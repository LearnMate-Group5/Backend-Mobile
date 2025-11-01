using FluentValidation;

namespace Application.BookChapters.Queries;

internal sealed class GetBookChaptersQueryValidator : AbstractValidator<GetBookChaptersQuery>
{
    public GetBookChaptersQueryValidator()
    {
        RuleFor(query => query.BookId)
            .NotEmpty();
    }
}
