using FluentValidation;

namespace Application.Users.Queries;

internal sealed class VerifyPasswordResetOtpQueryValidator : AbstractValidator<VerifyPasswordResetOtpQuery>
{
    public VerifyPasswordResetOtpQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Otp)
            .NotEmpty()
            .MaximumLength(32);
    }
}
