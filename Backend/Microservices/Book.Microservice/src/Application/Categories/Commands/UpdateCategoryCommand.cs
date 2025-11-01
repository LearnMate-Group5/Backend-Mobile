using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Categories.Commands;

public sealed record UpdateCategoryCommand(Guid CategoryId, string Name) : IRequest<Result<CategoryDto>>;
