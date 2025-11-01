using System.Threading;
using System.Threading.Tasks;
using Application.Categories;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Categories.Commands;

public sealed class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryRepository _repository;

    public UpdateCategoryCommandHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CategoryDto>> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (existing is null)
        {
            return Result.Failure<CategoryDto>(CategoryErrors.NotFound(request.CategoryId));
        }

        var normalizedName = request.Name.Trim();

        var duplicate = await _repository.NameExistsAsync(normalizedName, request.CategoryId, cancellationToken);
        if (duplicate)
        {
            return Result.Failure<CategoryDto>(CategoryErrors.DuplicateName(normalizedName));
        }

        existing.Name = normalizedName;
        await _repository.UpdateAsync(existing, cancellationToken);

        return Result.Success(CategoryDto.FromEntity(existing));
    }
}
