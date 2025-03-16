using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using Google.Apis.Auth;
using System.Security.Claims;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Queries.Identitiy;
public class GetLoginGoogleQueryHandler(
    IRepositoryBase<User, Guid> repositoryBase,
    IJwtTokenService jwtTokenService,
    IPasswordHasherService passwordHasherService) : IQueryHandler<Query.LoginGoogle, Response.Authenticated>
{
    public async Task<Result<Response.Authenticated>> Handle(Query.LoginGoogle request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        var payload = await GoogleJsonWebSignature.ValidateAsync(request.GoogleToken);
        if (payload == null)
        {
            return (Result<Response.Authenticated>)Result.Failure(new Error("404", "Invalid Google Token"));
        }

        var user = await repositoryBase.FindSingleAsync(x => x.Email == payload.Email, cancellationToken);
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = payload.Email,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                ProfilePicture = payload.Picture,
                Password = "1",
                Status = 1
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

        var accessToken = jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        var response = new Response.Authenticated
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = expirationTime
        };

        return Result.Success(response);
    }
}