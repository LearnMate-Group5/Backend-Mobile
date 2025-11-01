using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace Application.Books.Commands;

internal sealed class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Author)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Description)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(command => command.Categories)
            .NotNull()
            .Must(HasAtLeastOneCategory)
            .WithMessage("At least one category is required.")
            .Must(NoDuplicateCategories)
            .WithMessage("Categories must be unique (case-insensitive).");

        RuleForEach(command => command.Categories)
            .Must(category => category is null || !string.IsNullOrWhiteSpace(category))
            .WithMessage("Category cannot be empty.")
            .MaximumLength(100)
            .When(category => category is not null);

        RuleFor(command => command.ImageBase64)
            .Must(IsValidBase64)
            .WithMessage("Image data must be a valid Base64 string.");

        RuleFor(command => command.CreatedBy)
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

    private static bool HasAtLeastOneCategory(IReadOnlyCollection<string> categories)
    {
        return categories.Any(category => !string.IsNullOrWhiteSpace(category));
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
