using System;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Books.Commands;

public sealed record AddBookLikeCommand(Guid BookId, string UserId) : IRequest<Result<BookDto>>;

