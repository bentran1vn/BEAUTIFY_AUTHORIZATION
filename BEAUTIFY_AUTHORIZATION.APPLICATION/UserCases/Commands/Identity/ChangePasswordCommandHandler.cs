using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Entities;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Commands.Identity;

public class ChangePasswordCommandHandler : ICommandHandler<Command.ChangePasswordCommand>
{
    private readonly IRepositoryBase<User, Guid> _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly ICacheService _cacheService;

    public ChangePasswordCommandHandler(IRepositoryBase<User, Guid> userRepository, IPasswordHasherService passwordHasherService, ICacheService cacheService)
    {
        _userRepository = userRepository;
        _passwordHasherService = passwordHasherService;
        _cacheService = cacheService;
    }

    public async Task<Result> Handle(Command.ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user =
            await _userRepository.FindSingleAsync(x =>
                x.Email.Equals(request.Email), cancellationToken);
        
        if (user is null)
        {
            throw new Exception("User Not Existed !");
        }
        
        var hashingPassword = _passwordHasherService.HashPassword(request.NewPassword);

        user.Password = hashingPassword;
        
        await _cacheService.RemoveAsync($"{nameof(Query.Login)}-UserAccount:{user.Email}", cancellationToken);

        return Result.Success("Change Password Successfully !");
    }
}