using System;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Books.Queries;

public sealed record GetBookLikeCountQuery(Guid BookId) : IRequest<Result<int>>;

