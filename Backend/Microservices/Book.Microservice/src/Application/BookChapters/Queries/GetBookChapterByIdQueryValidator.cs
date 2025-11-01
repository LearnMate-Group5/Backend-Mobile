using FluentValidation;

namespace Application.BookChapters.Queries;

internal sealed class GetBookChapterByIdQueryValidator : AbstractValidator<GetBookChapterByIdQuery>
{
    public GetBookChapterByIdQueryValidator()
    {
        RuleFor(query => query.BookId)
            .NotEmpty();

        RuleFor(query => query.ChapterId)
            .NotEmpty();
    }
}
