using SharedLibrary.Common.ResponseModel;

namespace Application.BookChapters;

public static class BookChapterErrors
{
    public static Error NotFound(Guid chapterId) =>
        new("BookChapter.NotFound", $"Chapter with id '{chapterId}' was not found.");

    public static Error DuplicatePageIndex(Guid bookId, int pageIndex) =>
        new("BookChapter.DuplicatePageIndex", $"Chapter with page index '{pageIndex}' already exists for book '{bookId}'.");
}
