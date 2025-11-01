using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Books.Commands;

public sealed class CreateBookCommandHandler
    : IRequestHandler<CreateBookCommand, Result<BookDto>>
{
    private readonly IBookRepository _repository;

    public CreateBookCommandHandler(IBookRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<BookDto>> Handle(
        CreateBookCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedTitle = request.Title.Trim();

        var duplicateExists = await _repository.TitleExistsAsync(
            normalizedTitle,
            null,
            cancellationToken);

        if (duplicateExists)
        {
            return Result.Failure<BookDto>(BookErrors.DuplicateTitle(normalizedTitle));
        }

        var normalizedCategories = NormalizeCategories(request.Categories);

        var book = new Book
        {
            BookId = Guid.NewGuid(),
            Title = normalizedTitle,
            Author = request.Author.Trim(),
            Description = request.Description.Trim(),
            ImageBase64 = string.IsNullOrWhiteSpace(request.ImageBase64) ? null : request.ImageBase64,
            CreatedBy = request.CreatedBy.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _repository.CreateAsync(book, normalizedCategories, cancellationToken);

        return Result.Success(BookDto.FromEntity(book));
    }

    private static IReadOnlyList<string> NormalizeCategories(IEnumerable<string> categories)
    {
        return categories
            .Select(category => category?.Trim())
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(category => category!)
            .ToList();
    }
}





