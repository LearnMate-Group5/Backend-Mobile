using System;
using FluentValidation;

namespace Application.Subscriptions.Commands;

public sealed class ChooseSubscriptionPlanCommandValidator : AbstractValidator<ChooseSubscriptionPlanCommand>
{
    public ChooseSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty()
            .WithMessage("Subscription ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.")
            .MaximumLength(128)
            .WithMessage("User ID cannot exceed 128 characters.");
    }
}
