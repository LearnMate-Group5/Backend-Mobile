using FluentValidation;

namespace Application.Payments.Commands;

public class CreateZaloPayOrderCommandValidator : AbstractValidator<CreateZaloPayOrderCommand>
{
    public CreateZaloPayOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId (UserSubscriptionId) is required")
            .Must(orderId => Guid.TryParse(orderId, out _))
            .WithMessage("OrderId must be a valid UserSubscriptionId (Guid)");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
    }
}
