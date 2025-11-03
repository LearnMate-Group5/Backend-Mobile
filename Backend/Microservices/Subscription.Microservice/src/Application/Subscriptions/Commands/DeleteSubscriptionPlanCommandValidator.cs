using FluentValidation;

namespace Application.Subscriptions.Commands;

public sealed class DeleteSubscriptionPlanCommandValidator : AbstractValidator<DeleteSubscriptionPlanCommand>
{
    public DeleteSubscriptionPlanCommandValidator()
    {
        RuleFor(command => command.SubscriptionId)
            .NotEmpty();
    }
}
