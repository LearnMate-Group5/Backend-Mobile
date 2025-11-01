using System;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Books.Commands;

public sealed record AddBookViewCommand(Guid BookId, string ViewerId) : IRequest<Result<BookDto>>;

