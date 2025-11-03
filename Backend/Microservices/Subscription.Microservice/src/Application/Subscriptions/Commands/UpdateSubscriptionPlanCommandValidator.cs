using FluentValidation;

namespace Application.Subscriptions.Commands;

public sealed class UpdateSubscriptionPlanCommandValidator : AbstractValidator<UpdateSubscriptionPlanCommand>
{
    public UpdateSubscriptionPlanCommandValidator()
    {
        RuleFor(command => command.SubscriptionId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(200)
            .When(command => command.Name is not null);

        RuleFor(command => command.Type)
            .NotEmpty()
            .MaximumLength(100)
            .When(command => command.Type is not null);

        RuleFor(command => command.Status)
            .NotEmpty()
            .MaximumLength(100)
            .When(command => command.Status is not null);

        RuleFor(command => command.OriginalPrice)
            .GreaterThanOrEqualTo(0)
            .PrecisionScale(18, 2, true)
            .When(command => command.OriginalPrice.HasValue);

        RuleFor(command => command.Discount)
            .InclusiveBetween(0, 100)
            .PrecisionScale(9, 2, true)
            .When(command => command.Discount.HasValue);
    }
}
