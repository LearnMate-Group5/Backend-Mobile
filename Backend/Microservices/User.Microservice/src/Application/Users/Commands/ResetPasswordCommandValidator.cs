using FluentValidation;

namespace Application.Users.Commands;

internal sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .Equal(x => x.NewPassword)
            .WithMessage("Password confirmation does not match the new password.");

        RuleFor(x => x)
            .Must(HasTokenOrOtp)
            .WithMessage("Either token or OTP must be provided.");

        RuleFor(x => x.Token)
            .NotEmpty()
            .When(x => string.IsNullOrWhiteSpace(x.Otp))
            .MaximumLength(200);

        RuleFor(x => x.Otp)
            .NotEmpty()
            .When(x => string.IsNullOrWhiteSpace(x.Token))
            .MaximumLength(32);
    }

    private static bool HasTokenOrOtp(ResetPasswordCommand command) =>
        !string.IsNullOrWhiteSpace(command.Token) ||
        !string.IsNullOrWhiteSpace(command.Otp);
}
