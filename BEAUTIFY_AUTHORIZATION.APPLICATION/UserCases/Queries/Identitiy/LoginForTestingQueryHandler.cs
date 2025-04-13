using System.Security.Claims;
using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Queries.Identitiy;
internal sealed class LoginForTestingQueryHandler(
    IRepositoryBase<User, Guid> userRepository,
    IJwtTokenService jwtTokenService)
    : IQueryHandler<Query.LoginForTesting, string>
{
    public async Task<Result<string>> Handle(Query.LoginForTesting request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindSingleAsync(x => x.Email == request.Email, cancellationToken);
        if (user is null)
            return Result.Failure<string>(new Error("404", "User Not Found"));
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, request.Email),
            new(ClaimTypes.Role, user.Role.Name),
            new("RoleId", user.Role.Id.ToString()),
            new("UserId", user.Id.ToString()),
            new(ClaimTypes.Name, request.Email),
            new(ClaimTypes.Expired, DateTime.UtcNow.AddHours(5).ToString("o")),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("Name", user.FullName),
            new("Email", user.Email),
            new("ProfilePicture", user.ProfilePicture ?? string.Empty),
            new("RoleName", user.Role.Name)
        };
        var accessToken = jwtTokenService.GenerateAccessToken(claims);
        return Result.Success(accessToken);
    }
}