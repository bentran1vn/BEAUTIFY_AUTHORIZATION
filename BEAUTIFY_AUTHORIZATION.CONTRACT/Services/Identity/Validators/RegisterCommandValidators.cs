using FluentValidation;

namespace BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity.Validators;
public class RegisterCommandValidators : AbstractValidator<Command.RegisterCommand>
{
    public RegisterCommandValidators()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .MaximumLength(20).WithMessage("Password must be at most 20 characters long");
            //.Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            // .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            //  .Matches(@"\d").WithMessage("Password must contain at least one digit")
            //  .Matches(@"[\W_]").WithMessage("Password must contain at least one special character");
            
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MinimumLength(2).WithMessage("First name must be at least 2 characters long")
            .MaximumLength(30).WithMessage("First name must not exceed 30 characters")
            .Matches(@"^[\p{L}\s]+$")
            .WithMessage("First name must contain only letters and spaces");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters long")
            .MaximumLength(30).WithMessage("Last name must not exceed 30 characters")
            .Matches(@"^[\p{L}\s]+$")
            .WithMessage("Last name must contain only letters and spaces");


        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .Must(BeAtLeast18YearsOld).WithMessage("User must be at least 18 years old");

        /*  RuleFor(x => x.Address)
              .NotEmpty()
              .MinimumLength(10).WithMessage("Address must be at least 10 characters long")
              .MaximumLength(100).WithMessage("Address must exceed 100 characters");*/
    }

    private static bool BeAtLeast18YearsOld(DateOnly dateOfBirth)
    {
        return dateOfBirth <= DateOnly.FromDateTime(DateTime.Today).AddYears(-18);
    }
}