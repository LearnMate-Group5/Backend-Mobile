using Application.Books;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.BookChapters.Queries;

public sealed class GetBookChapterByIdQueryHandler
    : IRequestHandler<GetBookChapterByIdQuery, Result<BookChapterDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IBookChapterRepository _chapterRepository;

    public GetBookChapterByIdQueryHandler(
        IBookRepository bookRepository,
        IBookChapterRepository chapterRepository)
    {
        _bookRepository = bookRepository;
        _chapterRepository = chapterRepository;
    }

    public async Task<Result<BookChapterDto>> Handle(
        GetBookChapterByIdQuery request,
        CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book is null)
        {
            return Result.Failure<BookChapterDto>(BookErrors.NotFound(request.BookId));
        }

        var chapter = await _chapterRepository.GetByIdAsync(request.ChapterId, cancellationToken);
        if (chapter is null || chapter.BookId != request.BookId)
        {
            return Result.Failure<BookChapterDto>(BookChapterErrors.NotFound(request.ChapterId));
        }

        return Result.Success(BookChapterDto.FromEntity(chapter));
    }
}
