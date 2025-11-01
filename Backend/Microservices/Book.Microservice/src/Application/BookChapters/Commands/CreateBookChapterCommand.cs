using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.BookChapters.Commands;

public sealed record CreateBookChapterCommand(
    Guid BookId,
    int PageIndex,
    string Title,
    string Content,
    string CreatedBy) : IRequest<Result<BookChapterDto>>;
