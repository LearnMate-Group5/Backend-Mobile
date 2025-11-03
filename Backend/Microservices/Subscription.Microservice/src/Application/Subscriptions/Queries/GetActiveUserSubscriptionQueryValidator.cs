using FluentValidation;

namespace Application.Subscriptions.Queries;

public sealed class GetActiveUserSubscriptionQueryValidator : AbstractValidator<GetActiveUserSubscriptionQuery>
{
    public GetActiveUserSubscriptionQueryValidator()
    {
        RuleFor(query => query.UserId)
            .NotEmpty();
    }
}

