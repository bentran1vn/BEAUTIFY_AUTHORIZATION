using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Queries.Identitiy;

public class GetLoginQueryHandler : IQueryHandler<Query.Login, Response.Authenticated>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ICacheService _cacheService;
    private readonly IRepositoryBase<User, Guid> _userRepository;
    private readonly IRepositoryBase<UserClinic, Guid> _userClinicRepository;
    private readonly IPasswordHasherService _passwordHasherService;

    public GetLoginQueryHandler(IJwtTokenService jwtTokenService, ICacheService cacheService, IRepositoryBase<User, Guid> userRepository, IPasswordHasherService passwordHasherService, IRepositoryBase<UserClinic, Guid> userClinicRepository)
    {
        _jwtTokenService = jwtTokenService;
        _cacheService = cacheService;
        _userRepository = userRepository;
        _passwordHasherService = passwordHasherService;
        _userClinicRepository = userClinicRepository;
    }

    public async Task<Result<Response.Authenticated>> Handle(Query.Login request, CancellationToken cancellationToken)
    {
        // Check User
        var user =
            await _userRepository.FindSingleAsync(x =>
                x.Email.Trim().ToLower().Equals(request.Email.Trim().ToLower()), cancellationToken, x => x.Role!);

        if (user == null)
        {
            return Result.Failure<Response.Authenticated>(new Error("404", "User Not Found"));
        }

        if (!_passwordHasherService.VerifyPassword(request.Password, user.Password))
        {
            throw new UnauthorizedAccessException("UnAuthorize !");
        }

        // Generate JWT Token
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, request.Email),
            new(ClaimTypes.Role, user.Role!.Name),
            new("Role", user.Role!.Name),
            new("RoleId", user.Role!.Id.ToString()),
            new("UserId", user.Id.ToString()),
            new(ClaimTypes.Name, request.Email),
            new(ClaimTypes.Expired, DateTime.Now.AddMinutes(5).ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        
        if (user.Role!.Name.Equals("Clinic Admin"))
        {
            var mainClinicOwner = await _userClinicRepository.FindSingleAsync(
                x => 
                    x.UserId.Equals(user.Id) &&
                    x.Clinic != null &&
                    x.Clinic.IsParent != null &&
                    x.Clinic.IsParent.Value == true, cancellationToken);
            
            claims.Add(new("ClinicId",mainClinicOwner?.ClinicId.ToString() ?? ""));
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        var response = new Response.Authenticated()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTime.Now.AddHours(15)
        };

        var slidingExpiration = request.SlidingExpirationInMinutes == 0 ? 10 : request.SlidingExpirationInMinutes;
        var absoluteExpiration = request.AbsoluteExpirationInMinutes == 0 ? 15 : request.AbsoluteExpirationInMinutes;
        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(slidingExpiration))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(absoluteExpiration));

        await _cacheService.SetAsync($"{nameof(Query.Login)}-UserAccount:{request.Email}", response, options, cancellationToken);

        return Result.Success(response);
    }
}