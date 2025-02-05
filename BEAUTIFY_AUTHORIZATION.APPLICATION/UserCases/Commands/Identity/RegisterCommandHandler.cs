using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using Microsoft.Extensions.Caching.Distributed;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Commands.Identity;

public class RegisterCommandHandler : ICommandHandler<Command.RegisterCommand>
{
    private readonly IRepositoryBase<User, Guid> _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IMailService _mailService;
    private readonly ICacheService _cacheService;

    public RegisterCommandHandler(IRepositoryBase<User, Guid> userRepository, IPasswordHasherService passwordHasherService, IMailService mailService, ICacheService cacheService)
    {
        _userRepository = userRepository;
        _passwordHasherService = passwordHasherService;
        _mailService = mailService;
        _cacheService = cacheService;
    }

    public async Task<Result> Handle(Command.RegisterCommand request, CancellationToken cancellationToken)
    {
        var userExisted =
            await _userRepository.FindSingleAsync(x =>
                x.Email.Equals(request.Email) || x.PhoneNumber.Equals(request.PhoneNumber), cancellationToken);
        
        if (userExisted is not null && userExisted.Status == 1)
        {
            return Result.Failure(new Error("500", "Email already exists"));
        }
        
        if (userExisted is not null && userExisted.Email != request.Email)
        {
            return Result.Failure(new Error("500", "Email not match with this phone number"));
        }

        if (userExisted is null)
        {
            var hashingPassword = _passwordHasherService.HashPassword(request.Password);

            User newUser = new()
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Password = hashingPassword,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                Address = request.Address,
                Status = 0,
            };
        
            _userRepository.Add(newUser);
        }
        else
        {
            var hashingPassword = _passwordHasherService.HashPassword(request.Password);
            if (hashingPassword != userExisted.Password)
            {
                userExisted.Password = hashingPassword;
            }

            if (userExisted.Address != request.Address)
            {
                userExisted.Address = request.Address;
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
        
        Random random = new Random();
        var randomNumber = random.Next(0, 100000).ToString("D5");

        var slidingExpiration = 60;
        var absoluteExpiration = 60;
        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(slidingExpiration))
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(absoluteExpiration));
        
        await _mailService.SendMail(new MailContent
        {
            To = request.Email,
            Subject = $"Register Verify Code",
            Body = $@"
            <p>Dear {request.Email},</p>
            <p>Your register verify code is: {randomNumber}</p>
            <p>You have 60 seconds for insert !</p>
            ",
           
        });
        
        await _cacheService.SetAsync($"{nameof(Command.RegisterCommand)}-UserEmail:{request.Email}", randomNumber, options, cancellationToken);

        

        return Result.Success("Register Successfully !");
    }
}