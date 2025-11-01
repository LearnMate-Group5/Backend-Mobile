using System.Linq;
using Application.Books;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.BookChapters.Commands;

public sealed class CreateBookChapterCommandHandler
    : IRequestHandler<CreateBookChapterCommand, Result<BookChapterDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IBookChapterRepository _chapterRepository;

    public CreateBookChapterCommandHandler(
        IBookRepository bookRepository,
        IBookChapterRepository chapterRepository)
    {
        _bookRepository = bookRepository;
        _chapterRepository = chapterRepository;
    }

    public async Task<Result<BookChapterDto>> Handle(
        CreateBookChapterCommand request,
        CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book is null)
        {
            return Result.Failure<BookChapterDto>(BookErrors.NotFound(request.BookId));
        }

        var existingChapters = await _chapterRepository.GetByBookIdAsync(request.BookId, cancellationToken);
        if (existingChapters.Any(chapter => chapter.PageIndex == request.PageIndex))
        {
            return Result.Failure<BookChapterDto>(BookChapterErrors.DuplicatePageIndex(request.BookId, request.PageIndex));
        }

        var chapter = new BookChapter
        {
            ChapterId = Guid.NewGuid(),
            BookId = request.BookId,
            PageIndex = request.PageIndex,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            CreatedBy = request.CreatedBy.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _chapterRepository.CreateAsync(chapter, cancellationToken);

        return Result.Success(BookChapterDto.FromEntity(chapter));
    }
}
