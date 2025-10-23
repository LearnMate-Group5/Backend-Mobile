using FluentValidation;

namespace Application.AiWebhook.Commands;

public class UploadAndTranslateCommandValidator : AbstractValidator<UploadAndTranslateCommand>
{
    public UploadAndTranslateCommandValidator()
    {
        RuleFor(command => command.FileStream)
            .NotNull().WithMessage("A file must be provided.");

        RuleFor(command => command.FileName)
            .NotEmpty().WithMessage("File name is required.");

        RuleFor(command => command.ContentType)
            .NotEmpty().WithMessage("Content type is required.");

        RuleFor(command => command.UserId)
            .NotEmpty().WithMessage("User id is required.");
    }
}

