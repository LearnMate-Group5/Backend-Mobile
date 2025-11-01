using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Categories.Queries;

public sealed class GetBookNamesByCategoryIdQueryHandler
    : IRequestHandler<GetBookNamesByCategoryIdQuery, Result<IReadOnlyList<string>>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetBookNamesByCategoryIdQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<IReadOnlyList<string>>> Handle(
        GetBookNamesByCategoryIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
        {
            return Result.Failure<IReadOnlyList<string>>(CategoryErrors.NotFound(request.CategoryId));
        }

        var bookNames = category.BookCategories
            ?.Select(bookCategory => bookCategory.Book?.Title)
            .Where(title => !string.IsNullOrWhiteSpace(title))
            .Select(title => title!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(title => title, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var readOnlyNames = (bookNames ?? new List<string>()).AsReadOnly();

        return Result.Success<IReadOnlyList<string>>(readOnlyNames);
    }
}


