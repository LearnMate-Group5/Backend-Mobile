using FluentValidation;

namespace Application.BookChapters.Commands;

internal sealed class CreateBookChapterCommandValidator : AbstractValidator<CreateBookChapterCommand>
{
    public CreateBookChapterCommandValidator()
    {
        RuleFor(command => command.BookId)
            .NotEmpty();

        RuleFor(command => command.PageIndex)
            .GreaterThanOrEqualTo(0);

        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Content)
            .NotEmpty()
            .MaximumLength(8000);

        RuleFor(command => command.CreatedBy)
            .NotEmpty()
            .MaximumLength(128);
    }
}
