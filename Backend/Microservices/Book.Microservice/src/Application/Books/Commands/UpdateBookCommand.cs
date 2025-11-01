using System.Collections.Generic;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Books.Commands;

public sealed record UpdateBookCommand(
    Guid BookId,
    string? Title,
    string? Author,
    string? Description,
    string? ImageBase64,
    IReadOnlyCollection<string>? Categories,
    bool? IsActive,
    string UpdatedBy,
    bool ClearImage) : IRequest<Result<BookDto>>;
