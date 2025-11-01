using System;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Books.Queries;

public sealed record GetBooksQuery(bool OnlyActive, Guid? CategoryId, string? CategoryName) : IRequest<Result<IReadOnlyList<BookDto>>>;
