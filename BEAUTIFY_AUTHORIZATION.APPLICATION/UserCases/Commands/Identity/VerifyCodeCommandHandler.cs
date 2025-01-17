using System.Security.Claims;
using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Commands.Identity;

public class VerifyCodeCommandHandler : ICommandHandler<Command.VerifyCodeCommand>
{
    private readonly ICacheService _cacheService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRepositoryBase<User, Guid> _userRepository;

    public VerifyCodeCommandHandler(ICacheService cacheService, IJwtTokenService jwtTokenService, IRepositoryBase<User, Guid> userRepository)
    {
        _cacheService = cacheService;
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(Command.VerifyCodeCommand request, CancellationToken cancellationToken)
    {
        var user =
            await _userRepository.FindSingleAsync(x =>
                x.Email.Equals(request.Email), cancellationToken);
        
        if (user is null)
        {
            throw new Exception("User Not Existed !");
        }
        
        var code = await _cacheService.GetAsync<string>(
            $"{nameof(Command.ForgotPasswordCommand)}-UserAccount:{request.Email}", cancellationToken);

        if (code == null || !code.Equals(request.Code))
        {
            return Result.Failure(new Error("500", "Verify Code is Wrong !"));
        }
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("Role", user.Role.ToString()),
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Expired, DateTime.Now.AddMinutes(5).ToString())
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
        
        await _cacheService.SetAsync($"{nameof(Query.Login)}-UserAccount:{user.Email}", response, options, cancellationToken);

        return Result.Success(response);
    }
}