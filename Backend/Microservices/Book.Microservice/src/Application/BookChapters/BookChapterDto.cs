using Domain.Entities;

namespace Application.BookChapters;

public sealed record BookChapterDto(
    Guid ChapterId,
    Guid BookId,
    int PageIndex,
    string Title,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string CreatedBy,
    string? UpdatedBy)
{
    public static BookChapterDto FromEntity(BookChapter chapter) =>
        new(
            chapter.ChapterId,
            chapter.BookId,
            chapter.PageIndex,
            chapter.Title,
            chapter.Content,
            chapter.CreatedAt,
            chapter.UpdatedAt,
            chapter.CreatedBy,
            chapter.UpdatedBy);
}

