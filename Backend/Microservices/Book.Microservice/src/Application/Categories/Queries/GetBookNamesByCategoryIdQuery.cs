using System;
using System.Collections.Generic;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Categories.Queries;

public sealed record GetBookNamesByCategoryIdQuery(Guid CategoryId) : IRequest<Result<IReadOnlyList<string>>>;
