using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Categories.Commands;

public sealed record CreateCategoryCommand(string Name) : IRequest<Result<CategoryDto>>;
