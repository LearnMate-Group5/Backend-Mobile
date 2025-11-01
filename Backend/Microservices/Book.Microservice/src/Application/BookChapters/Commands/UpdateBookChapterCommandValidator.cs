using FluentValidation;

namespace Application.BookChapters.Commands;

internal sealed class UpdateBookChapterCommandValidator : AbstractValidator<UpdateBookChapterCommand>
{
    public UpdateBookChapterCommandValidator()
    {
        RuleFor(command => command.BookId)
            .NotEmpty();

        RuleFor(command => command.ChapterId)
            .NotEmpty();

        RuleFor(command => command.PageIndex)
            .GreaterThanOrEqualTo(0);

        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Content)
            .NotEmpty()
            .MaximumLength(8000);

        RuleFor(command => command.UpdatedBy)
            .NotEmpty()
            .MaximumLength(128);
    }
}
