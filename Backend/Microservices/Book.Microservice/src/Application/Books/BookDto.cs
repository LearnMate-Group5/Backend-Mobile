using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities;

namespace Application.Books;

public sealed record BookDto(
    Guid BookId,
    string Title,
    string Author,
    string Description,
    string? ImageBase64,
    IReadOnlyList<string> Categories,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy)
{
    public static BookDto FromEntity(Book book) =>
        new(
            book.BookId,
            book.Title,
            book.Author,
            book.Description,
            book.ImageBase64,
            ExtractCategoryNames(book),
            book.IsActive,
            book.CreatedAt,
            book.UpdatedAt,
            book.CreatedBy,
            book.UpdatedBy);

    private static IReadOnlyList<string> ExtractCategoryNames(Book book)
    {
        if (book.BookCategories is null || book.BookCategories.Count == 0)
        {
            return Array.Empty<string>();
        }

        return book.BookCategories
            .Where(bookCategory => bookCategory.Category != null && !string.IsNullOrWhiteSpace(bookCategory.Category.Name))
            .Select(bookCategory => bookCategory.Category!.Name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
