using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Books.Queries;

public sealed record GetBookByIdQuery(Guid BookId) : IRequest<Result<BookDto>>;
