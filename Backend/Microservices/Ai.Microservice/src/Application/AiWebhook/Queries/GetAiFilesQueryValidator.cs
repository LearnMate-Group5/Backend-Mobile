using FluentValidation;

namespace Application.AiWebhook.Queries;

public class GetAiFilesQueryValidator : AbstractValidator<GetAiFilesQuery>
{
    public GetAiFilesQueryValidator()
    {
        RuleFor(query => query.UserId)
            .NotEmpty().WithMessage("User id is required.");
    }
}
