using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Books.Commands;

public sealed class AddBookViewCommandHandler
    : IRequestHandler<AddBookViewCommand, Result<BookDto>>
{
    private readonly IBookRepository _repository;

    public AddBookViewCommandHandler(IBookRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<BookDto>> Handle(AddBookViewCommand request, CancellationToken cancellationToken)
    {
        var book = await _repository.GetByIdAsync(request.BookId, cancellationToken);

        if (book is null)
        {
            return Result.Failure<BookDto>(BookErrors.NotFound(request.BookId));
        }

        var normalizedViewerId = request.ViewerId.Trim();

        var alreadyViewed = await _repository.HasUserViewedAsync(request.BookId, normalizedViewerId, cancellationToken);
        if (alreadyViewed)
        {
            return Result.Success(BookDto.FromEntity(book));
        }

        var view = new BookView
        {
            BookViewId = Guid.NewGuid(),
            BookId = request.BookId,
            ViewerId = normalizedViewerId,
            ViewedAt = DateTime.UtcNow
        };

        await _repository.AddViewAsync(view, cancellationToken);

        var updated = await _repository.GetByIdAsync(request.BookId, cancellationToken);

        if (updated is null)
        {
            return Result.Failure<BookDto>(BookErrors.NotFound(request.BookId));
        }

        return Result.Success(BookDto.FromEntity(updated));
    }
}
