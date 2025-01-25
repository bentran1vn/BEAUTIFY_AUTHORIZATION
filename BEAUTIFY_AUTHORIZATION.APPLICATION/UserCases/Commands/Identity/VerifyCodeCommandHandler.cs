using BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.APPLICATION.Abstractions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Shared;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.UserCases.Commands.Identity;

public class VerifyCodeCommandHandler : ICommandHandler<Command.VerifyCodeCommand>
{
    private readonly ICacheService _cacheService;
    private readonly IRepositoryBase<User, Guid> _userRepository;

    public VerifyCodeCommandHandler(ICacheService cacheService, IRepositoryBase<User, Guid> userRepository)
    {
        _cacheService = cacheService;
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(Command.VerifyCodeCommand request, CancellationToken cancellationToken)
    {
        var user =
            await _userRepository.FindSingleAsync(x =>
                x.Email.Equals(request.Email), cancellationToken);

        if (user is null)
        {
            throw new Exception("User Not Existed !");
        }

        string? code = null;

        if (request.Type == 0)
        {
            code = await _cacheService.GetAsync<string>(
                $"{nameof(Command.RegisterCommand)}-UserEmail:{request.Email}", cancellationToken);
        }

        if (request.Type == 1)
        {
            code = await _cacheService.GetAsync<string>(
                        $"{nameof(Command.ForgotPasswordCommand)}-UserAccount:{request.Email}", cancellationToken);
        }
        

        if (code == null || !code.Equals(request.Code))
        {
            return Result.Failure(new Error("500", "Verify Code is Wrong !"));
        }

        user.Status = 1;

        return Result.Success("Verify Successfully !");
    }
}