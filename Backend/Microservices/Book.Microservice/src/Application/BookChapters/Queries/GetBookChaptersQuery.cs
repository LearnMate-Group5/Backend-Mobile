using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.BookChapters.Queries;

public sealed record GetBookChaptersQuery(Guid BookId) : IRequest<Result<IReadOnlyList<BookChapterDto>>>;
