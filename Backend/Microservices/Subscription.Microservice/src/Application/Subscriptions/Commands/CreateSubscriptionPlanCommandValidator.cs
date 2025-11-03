using FluentValidation;

namespace Application.Subscriptions.Commands;

public sealed class CreateSubscriptionPlanCommandValidator : AbstractValidator<CreateSubscriptionPlanCommand>
{
    public CreateSubscriptionPlanCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Type)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Status)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.OriginalPrice)
            .GreaterThanOrEqualTo(0)
            .PrecisionScale(18, 2, true);

        RuleFor(command => command.Discount)
            .InclusiveBetween(0, 100)
            .PrecisionScale(9, 2, true);
    }
}
