using FluentValidation;

namespace Application.Subscriptions.Queries;

public sealed class GetUserSubscriptionsQueryValidator : AbstractValidator<GetUserSubscriptionsQuery>
{
    public GetUserSubscriptionsQueryValidator()
    {
        RuleFor(query => query.UserId)
            .NotEmpty();
    }
}

