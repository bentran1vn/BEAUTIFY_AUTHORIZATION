using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Queries.Identitiy;
public class GetTokenQueryHandler(IJwtTokenService jwtTokenService, ICacheService cacheService)
    : IQueryHandler<Query.Token, Response.Authenticated>
{
    public async Task<Result<Response.Authenticated>> Handle(Query.Token request, CancellationToken cancellationToken)
    {
        var (claimPrincipal, isExpired) = jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var userAccount = claimPrincipal.Identity!.Name;
        var cacheData =
            await cacheService.GetAsync<Response.Authenticated>($"{nameof(Query.Login)}-UserAccount:{userAccount}",
                cancellationToken);

        if (cacheData == null || !cacheData.RefreshToken!.Equals(request.RefreshToken))
        {
            throw new Exception("Invalid refresh token");
        }

        /* if (!isExpired)
         {
             return Result.Success(cacheData);
         }
        */
        var accessToken = jwtTokenService.GenerateAccessToken(claimPrincipal.Claims);
        var response = new Response.Authenticated()
        {
            AccessToken = accessToken,
            RefreshToken = cacheData.RefreshToken,
            RefreshTokenExpiryTime = DateTime.Now.AddDays(1)
        };

        await cacheService.SetAsync($"Login-UserAccount:{userAccount}", response, null,
            cancellationToken);

        return Result.Success(response);
    }
}