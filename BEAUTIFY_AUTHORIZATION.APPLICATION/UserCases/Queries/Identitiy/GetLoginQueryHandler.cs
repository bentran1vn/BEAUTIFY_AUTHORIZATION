using System.Security.Claims;
using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Queries.Identitiy;
/// <summary>
/// Handles user login authentication and token generation
/// </summary>
public class GetLoginQueryHandler(
    IJwtTokenService jwtTokenService,
    ICacheService cacheService,
    IRepositoryBase<User, Guid> userRepository,
    IPasswordHasherService passwordHasherService)
    : IQueryHandler<Query.Login, Response.Authenticated>
{
    public async Task<Result<Response.Authenticated>> Handle(Query.Login request, CancellationToken cancellationToken)
    {
        try
        {
            // Normalize email to improve matching and prevent case-sensitivity issues
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            // Find authenticated user (either regular user or staff)
            var authenticatedUser =
                await FindAuthenticatedUserAsync(normalizedEmail, request.Password, cancellationToken);
            if (authenticatedUser.IsFailure)
                return Result.Failure<Response.Authenticated>(authenticatedUser.Error);

            var user = authenticatedUser.Value;

            // Generate claims for the authenticated user
            var claims = GenerateBaseClaims(user);

            // Generate tokens and create response
            return await GenerateAuthResponseAsync(claims, normalizedEmail, request, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log exception details here if needed
            return Result.Failure<Response.Authenticated>(new Error("500", $"Authentication failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Finds and authenticates a user by email and password
    /// </summary>
    private async Task<Result<dynamic>> FindAuthenticatedUserAsync(string normalizedEmail, string password,
        CancellationToken cancellationToken)
    {
        // Try to find user first
        var user = await userRepository
            .FindAll(x => EF.Functions.Like(x.Email.ToLower(), normalizedEmail) && !x.IsDeleted)
            .Select(x => new
            {
                UserId = x.Id,
                x.Email,
                x.Password,
                FullName = x.FirstName + " " + x.LastName,
                x.ProfilePicture,
                x.Status,
                x.PhoneNumber,
                Role = new
                {
                    x.Role!.Id,
                    x.Role.Name
                }
            })
            .FirstOrDefaultAsync(cancellationToken);

        // If user not found, try staff
        if (user is null)
            return Result.Failure<dynamic>(new Error("404", "User Not Found"));

        if (user.Status == 0)
            return Result.Failure<dynamic>(new Error("400", "User Not Verified"));

        // Verify password and user status
        return !passwordHasherService.VerifyPassword(password, user.Password)
            ? Result.Failure<dynamic>(new Error("401", "Wrong password"))
            : Result.Success<dynamic>(user);
    }

    /// <summary>
    /// Generates base claims for the authenticated user
    /// </summary>
    private static List<Claim> GenerateBaseClaims(dynamic user)
    {
        return new List<Claim>(15) // Pre-allocate capacity for better performance
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.Name),
            new("RoleId", user.Role.Id.ToString()),
            new("UserId", user.UserId.ToString()),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Expired, DateTime.UtcNow.AddHours(5).ToString("o")),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new("Name", user.FullName),
            new("Email", user.Email),
            new("ProfilePicture", user.ProfilePicture ?? string.Empty),
            new("RoleName", user.Role.Name),
            new("PhoneNumber", user.PhoneNumber ?? string.Empty),
        };
    }

    /// <summary>
    /// Generates authentication response with tokens and caches it
    /// </summary>
    private async Task<Result<Response.Authenticated>> GenerateAuthResponseAsync(
        List<Claim> claims,
        string normalizedEmail,
        Query.Login request,
        CancellationToken cancellationToken)
    {
        // Generate tokens
        var accessToken = jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        // Create authentication response
        var response = new Response.Authenticated
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            //  RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(15)
            RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(1)
        };

        // Configure cache options using request parameters or defaults
        var slidingExpiration = request.SlidingExpirationInMinutes > 0 ? request.SlidingExpirationInMinutes : 10;
        var absoluteExpiration = request.AbsoluteExpirationInMinutes > 0 ? request.AbsoluteExpirationInMinutes : 15;

        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(slidingExpiration))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(absoluteExpiration));

        // Cache the authentication response
        var cacheKey = $"Login-UserAccount:{normalizedEmail}";
        await cacheService.SetAsync(cacheKey, response, options, cancellationToken);

        return Result.Success(response);
    }
}