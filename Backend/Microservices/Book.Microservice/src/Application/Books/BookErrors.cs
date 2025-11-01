using SharedLibrary.Common.ResponseModel;

namespace Application.Books;

public static class BookErrors
{
    public static Error NotFound(Guid bookId) =>
        new("Book.NotFound", $"Book with id '{bookId}' was not found.");

    public static Error DuplicateTitle(string title) =>
        new("Book.DuplicateTitle", $"A book with title '{title}' already exists.");

    public static Error InvalidOperation(string message) =>
        new("Book.InvalidOperation", message);
}
