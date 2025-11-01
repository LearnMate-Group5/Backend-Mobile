using System;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Categories.Queries;

public sealed record GetCategoryByIdQuery(Guid CategoryId) : IRequest<Result<CategoryDto>>;
