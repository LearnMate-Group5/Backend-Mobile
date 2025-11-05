using FluentValidation;

namespace Application.Payments.Commands;

public class CreateMoMoOrderCommandValidator : AbstractValidator<CreateMoMoOrderCommand>
{
    public CreateMoMoOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId (UserSubscriptionId) is required")
            .Must(orderId => Guid.TryParse(orderId, out _))
            .WithMessage("OrderId must be a valid UserSubscriptionId (Guid)");

        RuleFor(x => x.OrderInfo)
            .NotEmpty().WithMessage("OrderInfo is required")
            .MaximumLength(500).WithMessage("OrderInfo must not exceed 500 characters");

        RuleFor(x => x.Lang)
            .NotEmpty().WithMessage("Lang is required")
            .Must(lang => lang == "vi" || lang == "en")
            .WithMessage("Lang must be either 'vi' or 'en'");
    }
}
