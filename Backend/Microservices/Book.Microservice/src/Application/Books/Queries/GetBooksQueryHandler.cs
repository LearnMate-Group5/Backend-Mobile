using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Books.Queries;

public sealed class GetBooksQueryHandler
    : IRequestHandler<GetBooksQuery, Result<IReadOnlyList<BookDto>>>
{
    private readonly IBookRepository _repository;

    public GetBooksQueryHandler(IBookRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<BookDto>>> Handle(
        GetBooksQuery request,
        CancellationToken cancellationToken)
    {
        var books = await _repository.GetAllAsync(cancellationToken);

        IEnumerable<Book> filtered = request.OnlyActive
            ? books.Where(book => book.IsActive)
            : books.AsEnumerable();

        if (request.CategoryId.HasValue)
        {
            filtered = filtered.Where(book =>
                book.BookCategories.Any(bookCategory => bookCategory.CategoryId == request.CategoryId.Value));
        }
        else if (!string.IsNullOrWhiteSpace(request.CategoryName))
        {
            var normalizedName = request.CategoryName.Trim();
            filtered = filtered.Where(book =>
                book.BookCategories.Any(bookCategory =>
                    bookCategory.Category != null &&
                    string.Equals(bookCategory.Category.Name, normalizedName, StringComparison.OrdinalIgnoreCase)));
        }

        var result = filtered
            .Select(BookDto.FromEntity)
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<BookDto>>(result);
    }
}





