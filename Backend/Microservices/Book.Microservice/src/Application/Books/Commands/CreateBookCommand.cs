using System.Collections.Generic;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Books.Commands;

public sealed record CreateBookCommand(
    string Title,
    string Author,
    string Description,
    string? ImageBase64,
    IReadOnlyCollection<string> Categories,
    string CreatedBy) : IRequest<Result<BookDto>>;
