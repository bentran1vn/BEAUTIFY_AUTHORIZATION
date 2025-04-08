using System.Security.Claims;
using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Queries.Identitiy;
public class GetLoginQueryHandler(
    IJwtTokenService jwtTokenService,
    ICacheService cacheService,
    IRepositoryBase<User, Guid> userRepository,
    IRepositoryBase<Staff, Guid> staffRepository,
    IPasswordHasherService passwordHasherService,
    IRepositoryBase<UserClinic, Guid> userClinicRepository,
    IRepositoryBase<SystemTransaction, Guid> systemTransactionRepository,
    IRepositoryBase<SubscriptionPackage, Guid> subscriptionPackageRepository)
    : IQueryHandler<Query.Login, Response.Authenticated>
{
    public async Task<Result<Response.Authenticated>> Handle(Query.Login request, CancellationToken cancellationToken)
    {
        var user = await userRepository
            .FindAll(x => EF.Functions.Like(x.Email.Trim(), request.Email.Trim()))
            .Select(x => new
            {
                UserId = x.Id,
                x.Email,
                x.Password,
                FullName = x.FirstName + " " + x.LastName,
                x.ProfilePicture,
                Role = new
                {
                    x.Role!.Id,
                    x.Role.Name
                }
            })
            .FirstOrDefaultAsync(cancellationToken);


        if (user is null)
        {
            // ✅ Directly project necessary fields for `staff` instead of manual mapping
            var staff = await staffRepository
                .FindAll(x => EF.Functions.Like(x.Email.Trim(), request.Email.Trim()))
                .Select(x => new
                {
                    UserId = x.Id,
                    x.Email,
                    x.Password,
                    FullName = x.FirstName + " " + x.LastName,
                    x.ProfilePicture,
                    Role = new
                    {
                        x.Role!.Id,
                        x.Role.Name
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (staff is null)
                return Result.Failure<Response.Authenticated>(new Error("404", "User Not Found"));

            user = staff with
            {
                Role = new
                {
                    staff.Role.Id,
                    staff.Role.Name
                }
            };
        }

        // ✅ Secure password check
        if (!passwordHasherService.VerifyPassword(request.Password, user.Password))
            return Result.Failure<Response.Authenticated>(new Error("401", "Wrong password"));

        // ✅ Generate JWT claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, request.Email),
            new(ClaimTypes.Role, user.Role.Name),
            new("RoleId", user.Role.Id.ToString()),
            new("UserId", user.UserId.ToString()),
            new(ClaimTypes.Name, request.Email),
            new(ClaimTypes.Expired, DateTime.UtcNow.AddHours(5).ToString("o")),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new("Name", user.FullName),
            new("Email", user.Email),
            new("ProfilePicture", user.ProfilePicture ?? string.Empty),
            new("RoleName", user.Role.Name)
        };

        // ✅ Handle Clinic Admin logic
        if (user.Role.Name is "Clinic Admin" or Constant.Role.CLINIC_STAFF)
        {
            var mainClinicOwner = await userClinicRepository.FindSingleAsync(
                x => x.UserId == user.UserId && x.Clinic != null,
                cancellationToken);

            if (mainClinicOwner is null)
                return Result.Failure<Response.Authenticated>(new Error("404", "Clinic Not Found"));

            claims.Add(new Claim("ClinicId", mainClinicOwner.ClinicId.ToString()));

            // ✅ Optimized transaction query
            var sub = await systemTransactionRepository
                .FindAll(x => x.ClinicId == mainClinicOwner.ClinicId && !x.IsDeleted)
                .OrderByDescending(x => x.TransactionDate)
                .Select(x => new
                {
                    x.SubscriptionPackageId,
                    x.SubscriptionPackage!.Name,
                    ExpiryDate = x.TransactionDate.AddDays(x.SubscriptionPackage.Duration)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (sub is not null)
            {
                claims.Add(new Claim("SubscriptionPackageId", sub.SubscriptionPackageId.ToString()));
                claims.Add(new Claim("SubscriptionPackageName", sub.Name));
                claims.Add(new Claim("SubscriptionPackageExpire", sub.ExpiryDate.ToString("o")));
            }
            else
            {
                //take minimum subscription package

                var subTrial = await subscriptionPackageRepository.FindAll().ToListAsync(cancellationToken);
                var minSub = subTrial.OrderBy(x => x.Price).FirstOrDefault();


                if (minSub is null)
                    return Result.Failure<Response.Authenticated>(new Error("404", "Subscription package Not Found"));

                claims.Add(new Claim("SubscriptionPackageId", minSub.Id.ToString()));
                claims.Add(new Claim("SubscriptionPackageName", minSub.Name));
            }
        }

        var accessToken = jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        var response = new Response.Authenticated
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(15)
        };

        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(request.SlidingExpirationInMinutes > 0
                ? request.SlidingExpirationInMinutes
                : 10))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(request.AbsoluteExpirationInMinutes > 0
                ? request.AbsoluteExpirationInMinutes
                : 15));

        await cacheService.SetAsync(
            $"Login:{request.Email}",
            response,
            options,
            cancellationToken);

        return Result.Success(response);
    }
}