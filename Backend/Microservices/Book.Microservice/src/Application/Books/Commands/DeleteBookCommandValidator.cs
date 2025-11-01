using FluentValidation;

namespace Application.Books.Commands;

internal sealed class DeleteBookCommandValidator : AbstractValidator<DeleteBookCommand>
{
    public DeleteBookCommandValidator()
    {
        RuleFor(command => command.BookId)
            .NotEmpty();
    }
}
