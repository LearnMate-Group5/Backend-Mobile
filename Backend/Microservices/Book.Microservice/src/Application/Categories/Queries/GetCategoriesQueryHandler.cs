using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Categories;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Categories.Queries;

public sealed class GetCategoriesQueryHandler
    : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    private readonly ICategoryRepository _repository;

    public GetCategoriesQueryHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _repository.GetAllAsync(cancellationToken);

        var result = categories
            .Select(CategoryDto.FromEntity)
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<CategoryDto>>(result);
    }
}
