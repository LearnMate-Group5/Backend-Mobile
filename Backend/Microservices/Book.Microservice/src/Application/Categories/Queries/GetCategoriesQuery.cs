using System.Collections.Generic;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Categories.Queries;

public sealed record GetCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryDto>>>;
