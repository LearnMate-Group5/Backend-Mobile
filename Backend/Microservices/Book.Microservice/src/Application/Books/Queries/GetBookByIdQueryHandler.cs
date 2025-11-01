using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Books.Queries;

public sealed class GetBookByIdQueryHandler
    : IRequestHandler<GetBookByIdQuery, Result<BookDto>>
{
    private readonly IBookRepository _repository;

    public GetBookByIdQueryHandler(IBookRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<BookDto>> Handle(
        GetBookByIdQuery request,
        CancellationToken cancellationToken)
    {
        var book = await _repository.GetByIdAsync(request.BookId, cancellationToken);
        if (book is null)
        {
            return Result.Failure<BookDto>(BookErrors.NotFound(request.BookId));
        }

        return Result.Success(BookDto.FromEntity(book));
    }
}
