using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.BookChapters.Queries;

public sealed record GetBookChapterByIdQuery(Guid BookId, Guid ChapterId) : IRequest<Result<BookChapterDto>>;
