using FluentValidation;

namespace BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity.Validators;
public class ChangePasswordCommandValidators : AbstractValidator<Command.ChangePasswordCommand>
{
    public ChangePasswordCommandValidators()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long");
    }
}