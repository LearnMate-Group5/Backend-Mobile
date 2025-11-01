using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Categories.Commands;

public sealed record DeleteCategoryCommand(Guid CategoryId) : IRequest<Result>;
