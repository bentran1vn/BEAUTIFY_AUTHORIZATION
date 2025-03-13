using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Abstractions.Messages;

namespace BEAUTIFY_AUTHORIZATION.CONTRACT.Services.Identity;
public static class Command
{
    public record ForgotPasswordCommand(
        string Email
    ) : ICommand;

    public record RegisterCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string PhoneNumber,
        DateOnly DateOfBirth,
        string? City,
        string? District,
        string? Ward,
        string? Address
    ) : ICommand;

    public record ChangePasswordCommand(
        string Email,
        string NewPassword
    ) : ICommand;

    public record ChangePasswordCommandBody(
        string NewPassword
    );

    public record VerifyCodeCommand(
        string Email,
        string Code,
        int Type // 0 Register, 1 Forgot
    ) : ICommand;

    public record LogoutCommand(
        string UserAccount
    ) : ICommand;
}