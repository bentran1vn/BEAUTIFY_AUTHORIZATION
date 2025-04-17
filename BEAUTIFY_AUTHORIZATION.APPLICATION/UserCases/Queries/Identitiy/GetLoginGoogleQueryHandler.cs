using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using Microsoft.Extensions.Caching.Distributed;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Queries.Identitiy;
public sealed class GetLoginGoogleQueryHandler(
    IRepositoryBase<User, Guid> userRepository,
    IJwtTokenService jwtTokenService,
    IMailService mailService,
    ICacheService cacheService,
    IRepositoryBase<Role, Guid> roleRepository,
    IPasswordHasherService passwordHasherService) : IQueryHandler<Query.LoginGoogleCommand, Response.Authenticated>
{
    // Reuse token handler instance for better performance
    private static readonly JwtSecurityTokenHandler TokenHandler = new();

    public async Task<Result<Response.Authenticated>> Handle(Query.LoginGoogleCommand request,
        CancellationToken cancellationToken)
    {
        // Validate token
        if (string.IsNullOrEmpty(request.GoogleToken))
            return Result.Failure<Response.Authenticated>(new Error("400", "Google token is required"));

        // Use static token handler for better performance
        if (TokenHandler.ReadToken(request.GoogleToken) is not JwtSecurityToken payload)
            return Result.Failure<Response.Authenticated>(new Error("401", "Invalid Google Token format"));

        // Extract claims
        var payloadData = payload.Claims.ToList();
        var email = payloadData.FirstOrDefault(c => c.Type == "email")?.Value;

        if (string.IsNullOrEmpty(email))
            return Result.Failure<Response.Authenticated>(new Error("400", "Email not found in token"));

        // Extract user metadata
        /* var userMetadataJson = payloadData.FirstOrDefault(c => c.Type == "user_metadata")?.Value;
         var userMetadata = userMetadataJson != null
             ? JsonSerializer.Deserialize<UserMetadata>(userMetadataJson)
             : null;*/
        var fullname = payloadData.FirstOrDefault(c => c.Type == "name")?.Value;

        // Get name components
        var (lastName, firstName) = SplitName(fullname);

        // Check if user exists
        var user = await userRepository.FindSingleAsync(x => x.Email == email, cancellationToken);
        var isNewUser = user is null;

        if (isNewUser)
        {
            // Generate secure password for new user
            var password = Random.Shared.Next(100000, 999999).ToString("D6");
            var hashPassword = passwordHasherService.HashPassword(password);

            // Get customer role
            var role = await roleRepository.FindSingleAsync(x => x.Name == Constant.Role.CUSTOMER, cancellationToken);
            if (role is null)
                return Result.Failure<Response.Authenticated>(new Error("404", "Customer role not found"));

            // Create new user
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                ProfilePicture = payloadData.FirstOrDefault(c => c.Type == "picture")?.Value ?? string.Empty,
                Password = hashPassword,
                Status = 1,
                Role = role
            };

            userRepository.Add(user);

            // Send welcome email only for new users
            await mailService.SendMail(new MailContent
            {
                To = user.Email,
                Subject = "Welcome to Beautify",
                Body =
                    $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #f0f0f0; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                    <div style='text-align: center; margin-bottom: 20px;'>
                        <h1 style='color: #d87093; margin: 0; padding: 0;'>Beautify</h1>
                        <p style='color: #888; font-size: 14px; margin: 5px 0;'>Your Beauty Journey Starts Here</p>
                    </div>
                    <div style='background-color: #fcf7f9; padding: 20px; border-radius: 6px;'>
                        <h2 style='color: #333; margin-top: 0;'>Welcome to Beautify!</h2>
                        <p style='color: #555; line-height: 1.5;'>Dear <span style='font-weight: bold; color: #d87093;'>{user.FullName}</span>,</p>
                        <p style='color: #555; line-height: 1.5;'>Thank you for joining our community. We're excited to have you on board!</p>
                        <p style='color: #555; line-height: 1.5;'>Your temporary password is:</p>
                        <div style='background-color: #fff; border: 1px dashed #d87093; padding: 10px; text-align: center; margin: 15px 0; border-radius: 4px;'>
                            <span style='font-family: monospace; font-size: 18px; font-weight: bold; color: #d87093;'>{password}</span>
                        </div>
                        <p style='color: #555; line-height: 1.5;'>For security reasons, we recommend changing your password after your first login.</p>
                        <p style='text-align: center; color: #888; font-size: 12px;'>© 2023 Beautify. All rights reserved.</p>
                    </div>
                </div>"
            });
        }

        // Ensure user has required properties
        if (user.Role == null)
            return Result.Failure<Response.Authenticated>(new Error("404", "User role not found"));

        // Create claims more efficiently
        var claims = new List<Claim>(11) // Pre-allocate capacity
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.Name),
            new("RoleId", user.Role.Id.ToString()),
            new("UserId", user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Expired, DateTime.UtcNow.AddHours(5).ToString("o")),
            new("Name", user.FullName ?? string.Empty),
            // Removed duplicate Email claim
            new("ProfilePicture", user.ProfilePicture ?? string.Empty),
            new("RoleName", user.Role.Name)
        };

        // Generate tokens
        var accessToken = jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        var response = new Response.Authenticated
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(5) // Set expiry time to match token
        };
        
        // Configure cache options using request parameters or defaults
        var slidingExpiration = request.SlidingExpirationInMinutes > 0 ? request.SlidingExpirationInMinutes : 10;
        var absoluteExpiration = request.AbsoluteExpirationInMinutes > 0 ? request.AbsoluteExpirationInMinutes : 15;

        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(slidingExpiration))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(absoluteExpiration));

        // Cache the authentication response
        var cacheKey = $"Login-UserAccount:{request.Email}";
        await cacheService.SetAsync(cacheKey, response, options, cancellationToken);

        return Result.Success(response);
    }

    /// <summary>
    /// Splits a full name into last name and first name components.
    /// In Vietnamese naming convention, the last part is the first name and everything before is the last name.
    /// </summary>
    /// <param name="fullName">The full name to split</param>
    /// <returns>A tuple containing (LastName, FirstName)</returns>
    private static (string LastName, string FirstName) SplitName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return ("", "");

        // Trim and split the name
        var nameParts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Handle edge cases
        if (nameParts.Length == 0)
            return ("", "");
        if (nameParts.Length == 1)
            return ("", nameParts[0]);

        // Last element is first name, everything else is last name
        return (string.Join(" ", nameParts[..^1]), nameParts[^1]);
    }
}