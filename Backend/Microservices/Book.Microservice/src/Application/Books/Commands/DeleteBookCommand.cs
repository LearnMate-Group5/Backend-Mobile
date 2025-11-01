using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Books.Commands;

public sealed record DeleteBookCommand(Guid BookId) : IRequest<Result>;
