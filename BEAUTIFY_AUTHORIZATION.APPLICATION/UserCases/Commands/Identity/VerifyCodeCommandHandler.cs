using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Commands.Identity;
public class VerifyCodeCommandHandler(
    ICacheService cacheService,
    IRepositoryBase<User, Guid> userRepository,
    IJwtTokenService jwtTokenService)
    : ICommandHandler<Command.VerifyCodeCommand>
{
    public async Task<Result> Handle(Command.VerifyCodeCommand request, CancellationToken cancellationToken)
    {
        var user =
            await userRepository.FindSingleAsync(x =>
                x.Email.Equals(request.Email), cancellationToken);

        if (user is null)
        {
            return Result.Failure(new Error("404", "User Not Existed !"));
        }

        var code = request.Type switch
        {
            0 => await cacheService.GetAsync<string>($"{nameof(Command.RegisterCommand)}-UserEmail:{request.Email}",
                cancellationToken),
            1 => await cacheService.GetAsync<string>(
                $"{nameof(Command.ForgotPasswordCommand)}-UserAccount:{request.Email}", cancellationToken),
            _ => null
        };

        if (code == null || !code.Equals(request.Code))
        {
            return Result.Failure(new Error("400", "Verify Code is Wrong !"));
        }

        if (request.Type == 0)
        {
            user.Status = 1;
            return Result.Success("Verify Successfully !");
        }

        ;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user?.Role?.ToString() ?? "UserRole"),
            new Claim("Role", user?.Role?.ToString() ?? "UserRole"),
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
            new Claim(ClaimTypes.Expired, DateTime.Now.AddHours(5).ToString())
        };

        var accessToken = jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        var response = new Response.Authenticated()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTime.Now.AddMinutes(15)
        };

        var slidingExpiration = 10;
        var absoluteExpiration = 15;
        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(slidingExpiration))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(absoluteExpiration));

        await cacheService.SetAsync($"{nameof(Query.Login)}-UserAccount:{user.Email}", response, options,
            cancellationToken);

        return Result.Success(response);
    }
}