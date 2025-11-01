using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Books.Commands;

public sealed class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand, Result>
{
    private readonly IBookRepository _repository;

    public DeleteBookCommandHandler(IBookRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.BookId, cancellationToken);
        if (existing is null)
        {
            return Result.Failure(BookErrors.NotFound(request.BookId));
        }

        await _repository.DeleteAsync(existing, cancellationToken);

        return Result.Success();
    }
}
