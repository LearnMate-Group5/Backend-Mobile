using FluentValidation;

namespace Application.Books.Queries;

internal sealed class GetBookByIdQueryValidator : AbstractValidator<GetBookByIdQuery>
{
    public GetBookByIdQueryValidator()
    {
        RuleFor(query => query.BookId)
            .NotEmpty();
    }
}
