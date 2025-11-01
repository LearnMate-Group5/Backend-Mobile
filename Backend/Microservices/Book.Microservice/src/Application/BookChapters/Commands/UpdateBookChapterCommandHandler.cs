using System.Linq;
using Application.Books;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.BookChapters.Commands;

public sealed class UpdateBookChapterCommandHandler
    : IRequestHandler<UpdateBookChapterCommand, Result<BookChapterDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IBookChapterRepository _chapterRepository;

    public UpdateBookChapterCommandHandler(
        IBookRepository bookRepository,
        IBookChapterRepository chapterRepository)
    {
        _bookRepository = bookRepository;
        _chapterRepository = chapterRepository;
    }

    public async Task<Result<BookChapterDto>> Handle(
        UpdateBookChapterCommand request,
        CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book is null)
        {
            return Result.Failure<BookChapterDto>(BookErrors.NotFound(request.BookId));
        }

        var existingChapter = await _chapterRepository.GetByIdAsync(request.ChapterId, cancellationToken);
        if (existingChapter is null || existingChapter.BookId != request.BookId)
        {
            return Result.Failure<BookChapterDto>(BookChapterErrors.NotFound(request.ChapterId));
        }

        var chapters = await _chapterRepository.GetByBookIdAsync(request.BookId, cancellationToken);
        if (chapters.Any(chapter => chapter.ChapterId != request.ChapterId && chapter.PageIndex == request.PageIndex))
        {
            return Result.Failure<BookChapterDto>(BookChapterErrors.DuplicatePageIndex(request.BookId, request.PageIndex));
        }

        existingChapter.PageIndex = request.PageIndex;
        existingChapter.Title = request.Title.Trim();
        existingChapter.Content = request.Content.Trim();
        existingChapter.UpdatedAt = DateTime.UtcNow;
        existingChapter.UpdatedBy = request.UpdatedBy.Trim();

        await _chapterRepository.UpdateAsync(existingChapter, cancellationToken);

        return Result.Success(BookChapterDto.FromEntity(existingChapter));
    }
}
