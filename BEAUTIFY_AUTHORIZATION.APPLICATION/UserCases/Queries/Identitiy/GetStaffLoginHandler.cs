using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using System.Security.Claims;
using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Queries.Identitiy;

public class GetStaffLoginHandler(IJwtTokenService jwtTokenService,
    ICacheService cacheService,
    IRepositoryBase<Staff, Guid> staffRepository,
    IPasswordHasherService passwordHasherService,
    IRepositoryBase<UserClinic, Guid> userClinicRepository,
    IRepositoryBase<SystemTransaction, Guid> systemTransactionRepository,
    IRepositoryBase<SubscriptionPackage, Guid> subscriptionPackageRepository) : IQueryHandler<Query.StaffLogin, Response.Authenticated>
{
    // Cache clinic role names for better performance
    private static readonly string[] ClinicRoles = [Constant.Role.CLINIC_ADMIN, Constant.Role.CLINIC_STAFF];
    
    public async Task<Result<Response.Authenticated>> Handle(Query.StaffLogin request, CancellationToken cancellationToken)
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

            // Add clinic-specific claims if user is a clinic admin or staff
            if (Array.IndexOf(ClinicRoles, user.Role.Name) < 0)
                return await GenerateAuthResponseAsync(claims, normalizedEmail, request, cancellationToken);
            
            var clinicResult = await AddClinicClaimsAsync(claims, user.UserId, cancellationToken);
            
            if (clinicResult.IsFailure)
                return Result.Failure<Response.Authenticated>(clinicResult.Error);

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
        var staff = await staffRepository
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
                x.CreatedOnUtc,
                Role = new
                {
                    x.Role!.Id,
                    x.Role.Name
                }
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (staff is null)
            return Result.Failure<dynamic>(new Error("404", "User Not Found"));

        if (staff.Status == 0)
            return Result.Failure<dynamic>(new Error("400", "User Not Verified"));

        // Verify password and user status
        return !passwordHasherService.VerifyPassword(password, staff.Password) ? Result.Failure<dynamic>(new Error("401", "Wrong password")) : Result.Success<dynamic>(staff);
    }


    /// <summary>
    /// Generates base claims for the authenticated user
    /// </summary>
    private static List<Claim> GenerateBaseClaims(dynamic user)
    {
        return new List<Claim>(16) // Pre-allocate capacity for better performance
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
            new("DateJoined", user.CreatedOnUtc.ToString("o")),
        };
    }

    /// <summary>
    /// Adds clinic-related claims for clinic users
    /// </summary>
    private async Task<Result> AddClinicClaimsAsync(List<Claim> claims, Guid userId,
        CancellationToken cancellationToken)
    {
        // Get clinic information for the user
        var mainClinicOwner = await userClinicRepository.FindSingleAsync(
            x => x.UserId == userId && x.Clinic != null,
            cancellationToken,
            y => y.Clinic);

        if (mainClinicOwner is null)
            return Result.Failure(new Error("404", "Clinic Not Found"));

        claims.Add(new Claim("ClinicId", mainClinicOwner.ClinicId.ToString()));

        // Check if clinic is activated
        if (mainClinicOwner.Clinic is { IsActivated: false })
            return Result.Failure(new Error("404", "Your clinic is not activated, please contact with email"));
        
        claims.Add(new Claim("IsFirstLogin", (mainClinicOwner.Clinic?.IsFirstLogin == null ? "false" :
                mainClinicOwner.Clinic.IsFirstLogin!.ToString())));
        
        // Add subscription information
        await AddSubscriptionClaimsAsync(claims, mainClinicOwner.ClinicId, cancellationToken);

        return Result.Success();
    }
    
    /// <summary>
    /// Adds subscription-related claims for clinic users
    /// </summary>
    private async Task AddSubscriptionClaimsAsync(List<Claim> claims, Guid clinicId,
        CancellationToken cancellationToken)
    {
        // Try to get the most recent transaction for the clinic
        var subscription = await systemTransactionRepository
            .FindAll(x => x.ClinicId == clinicId && !x.IsDeleted)
            .OrderByDescending(x => x.TransactionDate)
            .Select(x => new
            {
                x.SubscriptionPackageId,
                x.SubscriptionPackage!.Name,
                ExpiryDate = x.TransactionDate.AddDays(x.SubscriptionPackage.Duration)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription is not null)
        {
            // Add subscription information to claims
            claims.Add(new Claim("SubscriptionPackageId", subscription.SubscriptionPackageId.ToString()));
            claims.Add(new Claim("SubscriptionPackageName", subscription.Name));
            claims.Add(new Claim("SubscriptionPackageExpire", subscription.ExpiryDate.ToString("o")));
            return;
        }

        // If no subscription found, use the minimum price package as trial
        var minSubscription = await subscriptionPackageRepository
            .FindAll()
            .OrderBy(x => x.Price)
            .FirstOrDefaultAsync(cancellationToken);

        if (minSubscription is null)
            throw new InvalidOperationException("No subscription packages found in the system");

        claims.Add(new Claim("SubscriptionPackageId", minSubscription.Id.ToString()));
        claims.Add(new Claim("SubscriptionPackageName", minSubscription.Name));
    }
    
    /// <summary>
    /// Generates authentication response with tokens and caches it
    /// </summary>
    private async Task<Result<Response.Authenticated>> GenerateAuthResponseAsync(
        List<Claim> claims,
        string normalizedEmail,
        Query.StaffLogin request,
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
            RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(15)
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