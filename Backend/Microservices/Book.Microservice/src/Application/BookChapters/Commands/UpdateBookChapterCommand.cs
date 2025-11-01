using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.BookChapters.Commands;

public sealed record UpdateBookChapterCommand(
    Guid BookId,
    Guid ChapterId,
    int PageIndex,
    string Title,
    string Content,
    string UpdatedBy) : IRequest<Result<BookChapterDto>>;
