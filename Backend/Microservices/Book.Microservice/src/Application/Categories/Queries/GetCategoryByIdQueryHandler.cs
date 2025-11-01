using System.Threading;
using System.Threading.Tasks;
using Application.Categories;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Categories.Queries;

public sealed class GetCategoryByIdQueryHandler
    : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly ICategoryRepository _repository;

    public GetCategoryByIdQueryHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CategoryDto>> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
        {
            return Result.Failure<CategoryDto>(CategoryErrors.NotFound(request.CategoryId));
        }

        return Result.Success(CategoryDto.FromEntity(category));
    }
}
