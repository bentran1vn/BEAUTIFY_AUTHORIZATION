

using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;

namespace BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;

public static class Command
{
    public record ForgotPasswordCommand(
        string Email
    ) : ICommand;
    
    public record RegisterCommand(
        string Email, string Password,
        string FirstName, string LastName, string Phonenumber, int Role
    ) : ICommand;
    
    public record ChangePasswordCommand(
        string Email,
        string NewPassword
    ) : ICommand;
    
    public record VerifyCodeCommand(
        string Email,
        string Code
    ) : ICommand;
    
    public record LogoutCommand(
        string UserAccount
    ) : ICommand;
}