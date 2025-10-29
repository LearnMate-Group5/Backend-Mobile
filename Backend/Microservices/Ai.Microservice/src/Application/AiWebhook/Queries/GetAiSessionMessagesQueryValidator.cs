using FluentValidation;

namespace Application.AiWebhook.Queries;

public class GetAiSessionMessagesQueryValidator : AbstractValidator<GetAiSessionMessagesQuery>
{
    public GetAiSessionMessagesQueryValidator()
    {
        RuleFor(query => query.SessionId)
            .NotEmpty()
            .WithMessage("Session id is required.");

        RuleFor(query => query.RequestingUserId)
            .NotEmpty()
            .WithMessage("Requesting user id is required.");
    }
}
