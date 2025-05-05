using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Commands.Identity;
public class ChangePasswordCommandHandler(
    IRepositoryBase<User, Guid> userRepository,
    IPasswordHasherService passwordHasherService,
    ICacheService cacheService)
    : ICommandHandler<Command.ChangePasswordCommand>
{
    public async Task<Result> Handle(Command.ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user =
            await userRepository.FindSingleAsync(x =>
                x.Email.Equals(request.Email), cancellationToken);

        if (user is null)
        {
            throw new Exception("User Not Existed !");
        }

        var hashingPassword = passwordHasherService.HashPassword(request.NewPassword);

        user.Password = hashingPassword;

        await cacheService.RemoveAsync($"Login-UserAccount:{user.Email}", cancellationToken);

        return Result.Success("Change Password Successfully !");
    }
}