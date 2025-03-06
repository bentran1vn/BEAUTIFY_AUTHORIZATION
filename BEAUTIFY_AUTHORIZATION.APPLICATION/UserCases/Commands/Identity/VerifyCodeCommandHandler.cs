using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Commands.Identity;
public class VerifyCodeCommandHandler : ICommandHandler<Command.VerifyCodeCommand>
{
    private readonly ICacheService _cacheService;
    private readonly IRepositoryBase<User, Guid> _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public VerifyCodeCommandHandler(ICacheService cacheService, IRepositoryBase<User, Guid> userRepository,
        IJwtTokenService jwtTokenService)
    {
        _cacheService = cacheService;
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result> Handle(Command.VerifyCodeCommand request, CancellationToken cancellationToken)
    {
        var user =
            await _userRepository.FindSingleAsync(x =>
                x.Email.Equals(request.Email), cancellationToken);

        if (user is null)
        {
            return Result.Failure(new Error("400", "User Not Existed !"));
        }

        string? code = null;

        if (request.Type == 0)
        {
            code = await _cacheService.GetAsync<string>(
                $"{nameof(Command.RegisterCommand)}-UserEmail:{request.Email}", cancellationToken);
        }

        if (request.Type == 1)
        {
            code = await _cacheService.GetAsync<string>(
                $"{nameof(Command.ForgotPasswordCommand)}-UserAccount:{request.Email}", cancellationToken);
        }


        if (code == null || !code.Equals(request.Code))
        {
            return Result.Failure(new Error("500", "Verify Code is Wrong !"));
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

        var accessToken = _jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

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

        await _cacheService.SetAsync($"{nameof(Query.Login)}-UserAccount:{user.Email}", response, options,
            cancellationToken);

        return Result.Success(response);
    }
}