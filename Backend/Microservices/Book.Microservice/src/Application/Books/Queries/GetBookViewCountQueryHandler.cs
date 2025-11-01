using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Books.Queries;

public sealed class GetBookViewCountQueryHandler
    : IRequestHandler<GetBookViewCountQuery, Result<int>>
{
    private readonly IBookRepository _repository;

    public GetBookViewCountQueryHandler(IBookRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<int>> Handle(GetBookViewCountQuery request, CancellationToken cancellationToken)
    {
        var exists = await _repository.ExistsAsync(request.BookId, cancellationToken);
        if (!exists)
        {
            return Result.Failure<int>(BookErrors.NotFound(request.BookId));
        }

        var count = await _repository.GetViewCountAsync(request.BookId, cancellationToken);
        return Result.Success(count);
    }
}

