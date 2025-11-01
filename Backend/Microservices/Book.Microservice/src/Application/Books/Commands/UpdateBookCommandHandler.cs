using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Books.Commands;

public sealed class UpdateBookCommandHandler
    : IRequestHandler<UpdateBookCommand, Result<BookDto>>
{
    private readonly IBookRepository _repository;

    public UpdateBookCommandHandler(IBookRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<BookDto>> Handle(
        UpdateBookCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.BookId, cancellationToken);
        if (existing is null)
        {
            return Result.Failure<BookDto>(BookErrors.NotFound(request.BookId));
        }

        var normalizedTitle = request.Title?.Trim();
        if (!string.IsNullOrEmpty(normalizedTitle) &&
            !string.Equals(normalizedTitle, existing.Title, StringComparison.OrdinalIgnoreCase))
        {
            var duplicateTitle = await _repository.TitleExistsAsync(
                normalizedTitle,
                request.BookId,
                cancellationToken);

            if (duplicateTitle)
            {
                return Result.Failure<BookDto>(BookErrors.DuplicateTitle(normalizedTitle));
            }
        }

        if (!string.IsNullOrEmpty(normalizedTitle))
        {
            existing.Title = normalizedTitle;
        }

        if (!string.IsNullOrWhiteSpace(request.Author))
        {
            existing.Author = request.Author.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            existing.Description = request.Description.Trim();
        }

        if (request.ClearImage)
        {
            existing.ImageBase64 = null;
        }
        else if (!string.IsNullOrWhiteSpace(request.ImageBase64))
        {
            existing.ImageBase64 = request.ImageBase64;
        }

        if (request.IsActive.HasValue)
        {
            existing.IsActive = request.IsActive.Value;
        }

        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = request.UpdatedBy.Trim();

        IReadOnlyCollection<string>? normalizedCategories = request.Categories is null
            ? null
            : NormalizeCategories(request.Categories);

        await _repository.UpdateAsync(existing, normalizedCategories, cancellationToken);

        return Result.Success(BookDto.FromEntity(existing));
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




