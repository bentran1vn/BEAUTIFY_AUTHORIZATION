using FluentValidation;

namespace BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity.Validators;

public class ChangePasswordCommandValidators: AbstractValidator<Command.ChangePasswordCommand>
{
    public ChangePasswordCommandValidators()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("Invalid email format");
        
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"\d").WithMessage("Password must contain at least one digit")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character");
    }
}