using System.Threading;
using System.Threading.Tasks;
using Application.Categories;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Categories.Commands;

public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly ICategoryRepository _repository;

    public DeleteCategoryCommandHandler(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (existing is null)
        {
            return Result.Failure(CategoryErrors.NotFound(request.CategoryId));
        }

        await _repository.DeleteAsync(existing, cancellationToken);

        return Result.Success();
    }
}
