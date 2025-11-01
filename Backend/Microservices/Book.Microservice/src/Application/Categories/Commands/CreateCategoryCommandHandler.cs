using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Categories;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Categories.Commands;

public sealed class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryRepository _repository;

    public CreateCategoryCommandHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CategoryDto>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim();

        var exists = await _repository.NameExistsAsync(normalizedName, null, cancellationToken);
        if (exists)
        {
            return Result.Failure<CategoryDto>(CategoryErrors.DuplicateName(normalizedName));
        }

        var category = new Category
        {
            CategoryId = Guid.NewGuid(),
            Name = normalizedName
        };

        await _repository.CreateAsync(category, cancellationToken);

        return Result.Success(CategoryDto.FromEntity(category));
    }
}
