using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Commands.Identity;

public class LogoutCommandHandler(ICacheService cacheService) : ICommandHandler<Command.LogoutCommand>
{
    public async Task<Result> Handle(Command.LogoutCommand request, CancellationToken cancellationToken)
    {
        await cacheService.RemoveAsync($"{nameof(Query.Login)}-UserAccount:{request.UserAccount}", cancellationToken);
        return Result.Success("Logout Successfully");
    }
}