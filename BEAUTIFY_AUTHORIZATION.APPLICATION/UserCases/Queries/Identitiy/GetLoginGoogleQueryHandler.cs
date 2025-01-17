// using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
// using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
// using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
// using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
// using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
// using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Entities;
// using System.Security.Claims;
//
// namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Queries.Identitiy;
//
// public class GetLoginGoogleQueryHandler(IRepositoryBase<User, Guid> repositoryBase, IUnitOfWork eFUnitOfWork, IJwtTokenService jwtTokenService, IPasswordHasherService passwordHasherService) : IQueryHandler<Query.LoginGoogle, Response.Authenticated>
// {
//     public async Task<Result<Response.Authenticated>> Handle(Query.LoginGoogle request, CancellationToken cancellationToken)
//     {
//         var payload = await GoogleJsonWebSignature.ValidateAsync(request.GoogleToken);
//         if (payload == null)
//         {
//             return (Result<Response.Authenticated>)Result.Failure(new Error("404", "Invalid Google Token"));
//         }
//         var user = await repositoryBase.FindSingleAsync(x => x.Email == payload.Email, cancellationToken);
//         int status = 1;
//         int role = 0;
//         //Random random = new();
//         //var randomNumber = random.Next(0, 100000).ToString("D5");
//
//         //  Console.BackgroundColor = ConsoleColor.Red;
//         //  Console.WriteLine(randomNumber);
//         var hashedPassword = passwordHasherService.HashPassword("12345");
//
//         if (user == null)
//         {
//
//
//             List<string> emails =
//             [
//                 "nghi",
//                 "tan",
//                 "lam",
//                 "son",
//
//             ];
//             //payload.Email = phamphucnghi1706@gmail.com
//             if (emails.Exists(payload.Email.Contains))
//             {
//                 status = 0;
//                 role = 1;
//             }
//             user = new User
//             {
//                 Email = payload.Email,
//                 FullName = payload.Name,
//                 Points = 0,
//                 Role = role,
//                 Status = status,
//                 Password = hashedPassword,
//             };
//
//             repositoryBase.Add(user);
//             await eFUnitOfWork.SaveChangesAsync(cancellationToken);
//         }
//
//         var expirationTime = DateTime.Now.AddMinutes(5);
//         var claims = new List<Claim>
//         {
//             new(ClaimTypes.Email, user.Email),
//             new(ClaimTypes.Role, user.Role.ToString()),
//             new("Role", user.Role.ToString()),
//             new("UserId", user.Id.ToString()),
//             new(ClaimTypes.Name, user.Email),
//             new(ClaimTypes.Expired, expirationTime.ToString())
//         };
//
//         var accessToken = jwtTokenService.GenerateAccessToken(claims);
//         var refreshToken = jwtTokenService.GenerateRefreshToken();
//
//         var response = new Response.Authenticated
//         {
//             AccessToken = accessToken,
//             RefreshToken = refreshToken,
//             RefreshTokenExpiryTime = expirationTime
//         };
//
//         return Result.Success(response);
//     }
// }
