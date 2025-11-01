using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.BookChapters.Commands;

public sealed class DeleteBookChapterCommandHandler : IRequestHandler<DeleteBookChapterCommand, Result>
{
    private readonly IBookChapterRepository _chapterRepository;

    public DeleteBookChapterCommandHandler(IBookChapterRepository chapterRepository)
    {
        _chapterRepository = chapterRepository;
    }

    public async Task<Result> Handle(DeleteBookChapterCommand request, CancellationToken cancellationToken)
    {
        var existingChapter = await _chapterRepository.GetByIdAsync(request.ChapterId, cancellationToken);
        if (existingChapter is null || existingChapter.BookId != request.BookId)
        {
            return Result.Failure(BookChapterErrors.NotFound(request.ChapterId));
        }

        await _chapterRepository.DeleteAsync(existingChapter, cancellationToken);
        return Result.Success();
    }
}
