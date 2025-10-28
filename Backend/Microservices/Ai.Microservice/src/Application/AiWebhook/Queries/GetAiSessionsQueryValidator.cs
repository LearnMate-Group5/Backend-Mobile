using FluentValidation;

namespace Application.AiWebhook.Queries;

public class GetAiSessionsQueryValidator : AbstractValidator<GetAiSessionsQuery>
{
    public GetAiSessionsQueryValidator()
    {
        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage("User id is required.");
    }
}
