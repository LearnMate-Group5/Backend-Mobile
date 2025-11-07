using FluentValidation;

namespace Application.Users.Queries;

internal sealed class VerifyPasswordResetTokenQueryValidator : AbstractValidator<VerifyPasswordResetTokenQuery>
{
    public VerifyPasswordResetTokenQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Token)
            .NotEmpty()
            .MaximumLength(200);
    }
}
