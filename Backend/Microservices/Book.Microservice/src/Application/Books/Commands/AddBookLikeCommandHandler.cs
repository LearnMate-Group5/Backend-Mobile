using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Books.Commands;

public sealed class AddBookLikeCommandHandler
    : IRequestHandler<AddBookLikeCommand, Result<BookDto>>
{
    private readonly IBookRepository _repository;

    public AddBookLikeCommandHandler(IBookRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<BookDto>> Handle(AddBookLikeCommand request, CancellationToken cancellationToken)
    {
        var book = await _repository.GetByIdAsync(request.BookId, cancellationToken);

        if (book is null)
        {
            return Result.Failure<BookDto>(BookErrors.NotFound(request.BookId));
        }

        var normalizedUserId = request.UserId.Trim();

        var alreadyLiked = await _repository.HasUserLikedAsync(request.BookId, normalizedUserId, cancellationToken);
        if (alreadyLiked)
        {
            return Result.Failure<BookDto>(BookErrors.InvalidOperation("User has already liked this book."));
        }

        var like = new BookLike
        {
            BookLikeId = Guid.NewGuid(),
            BookId = request.BookId,
            UserId = normalizedUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddLikeAsync(like, cancellationToken);

        var updated = await _repository.GetByIdAsync(request.BookId, cancellationToken);

        if (updated is null)
        {
            return Result.Failure<BookDto>(BookErrors.NotFound(request.BookId));
        }

        return Result.Success(BookDto.FromEntity(updated));
    }
}

