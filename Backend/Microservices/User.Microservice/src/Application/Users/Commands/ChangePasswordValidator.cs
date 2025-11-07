using System;
using FluentValidation;

namespace Application.Users.Commands;

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.OldPassword)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .Must((command, confirm) =>
                string.Equals(confirm?.Trim(), command.NewPassword?.Trim(), StringComparison.Ordinal))
            .WithMessage("Password confirmation does not match.");

        RuleFor(x => x)
            .Must(command =>
                !string.Equals(command.OldPassword?.Trim(), command.NewPassword?.Trim(), StringComparison.Ordinal))
            .WithMessage("New password must be different from the current password.");
    }
}
