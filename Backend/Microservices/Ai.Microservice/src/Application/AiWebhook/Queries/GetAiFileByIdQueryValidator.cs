using FluentValidation;

namespace Application.AiWebhook.Queries;

public class GetAiFileByIdQueryValidator : AbstractValidator<GetAiFileByIdQuery>
{
    public GetAiFileByIdQueryValidator()
    {
        RuleFor(query => query.FileId)
            .NotEmpty()
            .WithMessage("File id is required.");
    }
}
