using System.Linq;
using Application.Books;
using Application.BookChapters.Commands;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.BookChapters.Queries;

public sealed class GetBookChaptersQueryHandler
    : IRequestHandler<GetBookChaptersQuery, Result<IReadOnlyList<BookChapterDto>>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IBookChapterRepository _chapterRepository;

    public GetBookChaptersQueryHandler(
        IBookRepository bookRepository,
        IBookChapterRepository chapterRepository)
    {
        _bookRepository = bookRepository;
        _chapterRepository = chapterRepository;
    }

    public async Task<Result<IReadOnlyList<BookChapterDto>>> Handle(
        GetBookChaptersQuery request,
        CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book is null)
        {
            return Result.Failure<IReadOnlyList<BookChapterDto>>(BookErrors.NotFound(request.BookId));
        }

        var chapters = await _chapterRepository.GetByBookIdAsync(request.BookId, cancellationToken);
        var response = chapters
            .OrderBy(chapter => chapter.PageIndex)
            .Select(BookChapterDto.FromEntity)
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<BookChapterDto>>(response);
    }
}
