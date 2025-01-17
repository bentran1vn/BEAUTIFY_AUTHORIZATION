// namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Queries.Identitiy;
//
// public class GetLogoutGoogleQueryHandler(IHttpContextAccessor _httpContext) : IQueryHandler<Query.LogoutGoogle, string>
// {
//     public async Task<Result<string>> Handle(Query.LogoutGoogle request, CancellationToken cancellationToken)
//     {
//         await _httpContext.HttpContext.SignOutAsync();
//         return Result.Success("Logged out");
//     }
// }
