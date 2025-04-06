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

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Queries.Identitiy;
public sealed class GetLoginGoogleQueryHandler(
    IRepositoryBase<User, Guid> repositoryBase,
    IJwtTokenService jwtTokenService,
    IMailService mailService,
    IRepositoryBase<Role, Guid> roleRepository,
    IPasswordHasherService passwordHasherService) : IQueryHandler<Query.LoginGoogleCommand, Response.Authenticated>
{
    public async Task<Result<Response.Authenticated>> Handle(Query.LoginGoogleCommand request, CancellationToken cancellationToken)
    {
        if (new JwtSecurityTokenHandler().ReadToken(request.GoogleToken) is not JwtSecurityToken payload)
            return Result.Failure<Response.Authenticated>(new Error("404", "Invalid Google Token"));
        var payloadData = payload.Claims.ToList();
        var email = payloadData.FirstOrDefault(c => c.Type == "email")?.Value;
        var userMetadata = JsonSerializer.Deserialize<UserMetadata>(payloadData.FirstOrDefault(c => c.Type == "user_metadata")?.Value);
        var (lastName, firstName) = SplitName(userMetadata?.FullName);

        var password = passwordHasherService.HashPassword(Random.Shared.Next(100000, 999999).ToString("D6"));
        var user = await repositoryBase.FindSingleAsync(x => x.Email == email, cancellationToken);
        if (user is null)
        {
            var role = await roleRepository.FindSingleAsync(x => x.Name == Constant.Role.CUSTOMER, cancellationToken);
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                ProfilePicture = userMetadata?.AvatarUrl,
                Password = password,
                Status = 1,
                Role = role
            };
            repositoryBase.Add(user);
        }

        var expirationTime = DateTime.Now.AddMinutes(5);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Expired, expirationTime.ToString())
        };
        await mailService.SendMail(new MailContent
        {
            To = user.Email,
            Subject = "Welcome to Beautify",
            Body = $@"<p>Dear {user.FullName},</p><p>Welcome to Beautify !</p><p>Your password is: {password}</p>"
        });

        return Result.Success(new Response.Authenticated
        {
            AccessToken = jwtTokenService.GenerateAccessToken(claims),
            RefreshToken = jwtTokenService.GenerateRefreshToken(),
            RefreshTokenExpiryTime = null
        });
    }

    private static (string LastName, string FirstName) SplitName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return ("", "");
        var nameParts = fullName.Trim().Split([' '], StringSplitOptions.RemoveEmptyEntries);
        return nameParts.Length == 0 ? ("", "") :
            (string.Join(" ", nameParts[..^1]), nameParts[^1]);
    }
}
