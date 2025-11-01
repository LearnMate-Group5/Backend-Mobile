using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.BookChapters.Commands;

public sealed record DeleteBookChapterCommand(Guid BookId, Guid ChapterId) : IRequest<Result>;
