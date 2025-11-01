using System.Linq;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BookChapterRepository : IBookChapterRepository
{
    private readonly MyDbContext _context;

    public BookChapterRepository(MyDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<BookChapter>> GetByBookIdAsync(Guid bookId, CancellationToken cancellationToken)
    {
        return await _context.BookChapters
            .AsNoTracking()
            .Where(chapter => chapter.BookId == bookId)
            .OrderBy(chapter => chapter.PageIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<BookChapter?> GetByIdAsync(Guid chapterId, CancellationToken cancellationToken)
    {
        return await _context.BookChapters
            .FirstOrDefaultAsync(chapter => chapter.ChapterId == chapterId, cancellationToken);
    }

    public async Task CreateAsync(BookChapter chapter, CancellationToken cancellationToken)
    {
        await _context.BookChapters.AddAsync(chapter, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(BookChapter chapter, CancellationToken cancellationToken)
    {
        _context.BookChapters.Update(chapter);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(BookChapter chapter, CancellationToken cancellationToken)
    {
        _context.BookChapters.Remove(chapter);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
