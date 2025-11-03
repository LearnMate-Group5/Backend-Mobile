using FluentValidation;

namespace Application.Subscriptions.Commands;

public sealed class CancelUserSubscriptionCommandValidator : AbstractValidator<CancelUserSubscriptionCommand>
{
    public CancelUserSubscriptionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.")
            .MaximumLength(128)
            .WithMessage("User ID cannot exceed 128 characters.");
    }
}
