using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace Application.Books.Commands;

internal sealed class UpdateBookCommandValidator : AbstractValidator<UpdateBookCommand>
{
    public UpdateBookCommandValidator()
    {
        RuleFor(command => command.BookId)
            .NotEmpty();

        RuleFor(command => command)
            .Must(HasAtLeastOneFieldToUpdate)
            .WithMessage("At least one field must be provided to update.");

        RuleFor(command => command.Title)
            .Must(title => title is null || !string.IsNullOrWhiteSpace(title))
            .WithMessage("Title cannot be empty when provided.")
            .MaximumLength(200)
            .When(command => command.Title is not null);

        RuleFor(command => command.Author)
            .Must(author => author is null || !string.IsNullOrWhiteSpace(author))
            .WithMessage("Author cannot be empty when provided.")
            .MaximumLength(200)
            .When(command => command.Author is not null);

        RuleFor(command => command.Description)
            .Must(description => description is null || !string.IsNullOrWhiteSpace(description))
            .WithMessage("Description cannot be empty when provided.")
            .MaximumLength(4000)
            .When(command => command.Description is not null);

        RuleFor(command => command.Categories)
            .Must(categories => categories is null || categories.Any(category => !string.IsNullOrWhiteSpace(category)))
            .WithMessage("At least one category must be provided when updating categories.")
            .Must(categories => categories is null || NoDuplicateCategories(categories))
            .WithMessage("Categories must be unique (case-insensitive).");

        RuleForEach(command => command.Categories)
            .Must(category => category is null || !string.IsNullOrWhiteSpace(category))
            .WithMessage("Category cannot be empty when provided.")
            .MaximumLength(100)
            .When(category => category is not null);

        RuleFor(command => command.ImageBase64)
            .Must(IsValidBase64)
            .WithMessage("Image data must be a valid Base64 string.")
            .When(command => command.ImageBase64 is not null);

        RuleFor(command => command.UpdatedBy)
            .NotEmpty()
            .MaximumLength(128);
    }

    private static bool IsValidBase64(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        Span<byte> buffer = stackalloc byte[value.Length];
        return Convert.TryFromBase64String(value, buffer, out _);
    }

    private static bool HasAtLeastOneFieldToUpdate(UpdateBookCommand command)
    {
        return command.Title is not null ||
               command.Author is not null ||
               command.Description is not null ||
               command.ImageBase64 is not null ||
               command.Categories is not null ||
               command.IsActive.HasValue ||
               command.ClearImage;
    }

    private static bool NoDuplicateCategories(IReadOnlyCollection<string> categories)
    {
        var normalized = categories
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Select(category => category.Trim())
            .ToArray();

        return normalized.Length == normalized.Distinct(StringComparer.OrdinalIgnoreCase).Count();
    }
}
