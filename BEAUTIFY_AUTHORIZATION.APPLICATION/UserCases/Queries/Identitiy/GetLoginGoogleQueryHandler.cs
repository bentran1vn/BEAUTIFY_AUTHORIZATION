﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Queries.Identitiy;
public sealed class GetLoginGoogleQueryHandler(
    IRepositoryBase<User, Guid> userRepository,
    IJwtTokenService jwtTokenService,
    IMailService mailService,
    IRepositoryBase<Role, Guid> roleRepository,
    IPasswordHasherService passwordHasherService) : IQueryHandler<Query.LoginGoogleCommand, Response.Authenticated>
{
    // Reuse token handler instance for better performance
    private static readonly JwtSecurityTokenHandler _tokenHandler = new();
    public async Task<Result<Response.Authenticated>> Handle(Query.LoginGoogleCommand request,
        CancellationToken cancellationToken)
    {
        // Validate token
        if (string.IsNullOrEmpty(request.GoogleToken))
            return Result.Failure<Response.Authenticated>(new Error("400", "Google token is required"));

        // Use static token handler for better performance
        if (_tokenHandler.ReadToken(request.GoogleToken) is not JwtSecurityToken payload)
            return Result.Failure<Response.Authenticated>(new Error("401", "Invalid Google Token format"));

        // Extract claims
        var payloadData = payload.Claims.ToList();
        var email = payloadData.FirstOrDefault(c => c.Type == "email")?.Value;

        if (string.IsNullOrEmpty(email))
            return Result.Failure<Response.Authenticated>(new Error("400", "Email not found in token"));

        // Extract user metadata
        var userMetadataJson = payloadData.FirstOrDefault(c => c.Type == "user_metadata")?.Value;
        var userMetadata = userMetadataJson != null
            ? JsonSerializer.Deserialize<UserMetadata>(userMetadataJson)
            : null;

        // Get name components
        var (lastName, firstName) = SplitName(userMetadata?.FullName);

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
                ProfilePicture = userMetadata?.AvatarUrl,
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
                Body = $@"<p>Dear {user.FullName},</p><p>Welcome to Beautify!</p><p>Your password is: {password}</p>"
            });
        }

        // Ensure user has required properties
        if (user.Role == null)
            return Result.Failure<Response.Authenticated>(new Error("500", "User role not found"));

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

        return Result.Success(new Response.Authenticated
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(5) // Set expiry time to match token
        });
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

        return nameParts.Length switch
        {
            // Handle edge cases
            0 => ("", ""),
            1 => ("", nameParts[0]),
            _ => (string.Join(" ", nameParts[..^1]), nameParts[^1])
        };

        // Last element is first name, everything else is last name
    }
}