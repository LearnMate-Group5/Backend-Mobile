using FluentValidation;

namespace Application.BookChapters.Commands;

internal sealed class DeleteBookChapterCommandValidator : AbstractValidator<DeleteBookChapterCommand>
{
    public DeleteBookChapterCommandValidator()
    {
        RuleFor(command => command.BookId)
            .NotEmpty();

        RuleFor(command => command.ChapterId)
            .NotEmpty();
    }
}
