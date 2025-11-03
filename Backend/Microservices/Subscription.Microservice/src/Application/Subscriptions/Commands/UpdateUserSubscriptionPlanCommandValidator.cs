using FluentValidation;

namespace Application.Subscriptions.Commands;

public sealed class UpdateUserSubscriptionPlanCommandValidator : AbstractValidator<UpdateUserSubscriptionPlanCommand>
{
    public UpdateUserSubscriptionPlanCommandValidator()
    {
        RuleFor(command => command.SubscriptionId)
            .NotEmpty();

        RuleFor(command => command.UserId)
            .NotEmpty();
    }
}

