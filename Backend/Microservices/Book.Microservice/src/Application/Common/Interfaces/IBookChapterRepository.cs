using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IBookChapterRepository
{
    Task<IReadOnlyList<BookChapter>> GetByBookIdAsync(Guid bookId, CancellationToken cancellationToken);
    Task<BookChapter?> GetByIdAsync(Guid chapterId, CancellationToken cancellationToken);
    Task CreateAsync(BookChapter chapter, CancellationToken cancellationToken);
    Task UpdateAsync(BookChapter chapter, CancellationToken cancellationToken);
    Task DeleteAsync(BookChapter chapter, CancellationToken cancellationToken);
}
