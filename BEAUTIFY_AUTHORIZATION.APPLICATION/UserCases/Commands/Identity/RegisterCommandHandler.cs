using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using Microsoft.Extensions.Caching.Distributed;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Commands.Identity;
public class RegisterCommandHandler(
    IRepositoryBase<User, Guid> userRepository,
    IPasswordHasherService passwordHasherService,
    IMailService mailService,
    ICacheService cacheService,
    IRepositoryBase<Role, Guid> roleRepository)
    : ICommandHandler<Command.RegisterCommand>
{
    public async Task<Result> Handle(Command.RegisterCommand request, CancellationToken cancellationToken)
    {
        var userExisted =
            await userRepository.FindSingleAsync(x =>
                x.Email.Equals(request.Email) || x.PhoneNumber.Equals(request.PhoneNumber), cancellationToken);

        if (userExisted is not null && userExisted.Status == 1)
        {
            return Result.Failure(new Error("400", "Email or phone number already exists"));
        }

        if (userExisted is { Status: 0 })
        {
            return Result.Failure(new Error("400", "Account not verified "));
        }

        if (userExisted is not null && userExisted.Email != request.Email)
        {
            return Result.Failure(new Error("400", "Email not match with this phone number"));
        }

        if (userExisted is null)
        {
            var hashingPassword = passwordHasherService.HashPassword(request.Password);
            var role = await roleRepository.FindSingleAsync(x => x.Name == Constant.Role.CUSTOMER, cancellationToken);

            User newUser = new()
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Password = hashingPassword,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                City = request.City,
                Ward = request.Ward,
                District = request.District,
                Status = 0,
                RoleId = role?.Id
            };

            userRepository.Add(newUser);
        }
        else
        {
            var hashingPassword = passwordHasherService.HashPassword(request.Password);
            if (hashingPassword != userExisted.Password)
            {
                userExisted.Password = hashingPassword;
            }


            if (userExisted.FirstName != request.FirstName)
            {
                userExisted.FirstName = request.FirstName;
            }

            if (userExisted.LastName != request.LastName)
            {
                userExisted.LastName = request.LastName;
            }

            if (!userExisted.DateOfBirth.Equals(request.DateOfBirth))
            {
                userExisted.DateOfBirth = request.DateOfBirth;
            }
        }

        var random = new Random();
        var randomNumber = random.Next(0, 100000).ToString("D5");

        var slidingExpiration = 600;
        var absoluteExpiration = 600;
        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(slidingExpiration))
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(absoluteExpiration));

        await mailService.SendMail(new MailContent
        {
            To = request.Email,
            Subject = $"Register Verify Code",
            Body = $@"
            <p>Dear {request.Email},</p>
            <p>Your register verify code is: {randomNumber}</p>
            <p>You have 60 seconds for insert !</p>
            ",
        });

        await cacheService.SetAsync($"{nameof(Command.RegisterCommand)}-UserEmail:{request.Email}", randomNumber,
            options, cancellationToken);


        return Result.Success("Register Successfully !");
    }
}