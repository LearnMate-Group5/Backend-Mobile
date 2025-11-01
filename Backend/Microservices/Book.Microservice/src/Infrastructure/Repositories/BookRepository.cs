using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly MyDbContext _context;

    public BookRepository(MyDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Books
            .AsNoTracking()
            .Include(book => book.BookCategories)
                .ThenInclude(bookCategory => bookCategory.Category)
            .OrderBy(book => book.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetByIdAsync(Guid bookId, CancellationToken cancellationToken)
    {
        return await _context.Books
            .AsNoTracking()
            .Include(book => book.BookCategories)
                .ThenInclude(bookCategory => bookCategory.Category)
            .FirstOrDefaultAsync(book => book.BookId == bookId, cancellationToken);
    }

    public async Task CreateAsync(Book book, IReadOnlyCollection<string> categoryNames, CancellationToken cancellationToken)
    {
        var categories = await GetOrCreateCategoriesAsync(categoryNames, cancellationToken);

        foreach (var category in categories)
        {
            book.BookCategories.Add(new BookCategory
            {
                Book = book,
                Category = category
            });
        }

        await _context.Books.AddAsync(book, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Book book, IReadOnlyCollection<string>? categoryNames, CancellationToken cancellationToken)
    {
        if (categoryNames is not null)
        {
            _context.Attach(book);
            var categories = await GetOrCreateCategoriesAsync(categoryNames, cancellationToken);

            await _context.Entry(book)
                .Collection(b => b.BookCategories)
                .Query()
                .Include(bookCategory => bookCategory.Category)
                .LoadAsync(cancellationToken);

            var categoryIds = categories
                .Select(category => category.CategoryId)
                .ToHashSet();

            var toRemove = book.BookCategories
                .Where(bookCategory => !categoryIds.Contains(bookCategory.CategoryId))
                .ToList();

            foreach (var bookCategory in toRemove)
            {
                book.BookCategories.Remove(bookCategory);
            }

            foreach (var category in categories)
            {
                if (!book.BookCategories.Any(bookCategory => bookCategory.CategoryId == category.CategoryId))
                {
                    book.BookCategories.Add(new BookCategory
                    {
                        BookId = book.BookId,
                        CategoryId = category.CategoryId,
                        Category = category
                    });
                }
                else
                {
                    var existingLink = book.BookCategories
                        .First(bookCategory => bookCategory.CategoryId == category.CategoryId);

                    if (existingLink.Category is null)
                    {
                        existingLink.Category = category;
                    }
                }
            }
        }

        _context.Books.Update(book);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Book book, CancellationToken cancellationToken)
    {
        _context.Books.Remove(book);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid bookId, CancellationToken cancellationToken)
    {
        return await _context.Books
            .AsNoTracking()
            .AnyAsync(book => book.BookId == bookId, cancellationToken);
    }

    public async Task<bool> TitleExistsAsync(string title, Guid? excludingBookId, CancellationToken cancellationToken)
    {
        var normalizedTitle = title.Trim();

        return await _context.Books
            .AsNoTracking()
            .AnyAsync(
                book =>
                    (!excludingBookId.HasValue || book.BookId != excludingBookId.Value) &&
                    EF.Functions.ILike(book.Title, normalizedTitle),
                cancellationToken);
    }

    private async Task<List<Category>> GetOrCreateCategoriesAsync(
        IReadOnlyCollection<string> categoryNames,
        CancellationToken cancellationToken)
    {
        var normalizedNames = categoryNames
            .Select(name => name.Trim())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedNames.Count == 0)
        {
            return new List<Category>();
        }

        var normalizedLookup = normalizedNames
            .Select(name => new
            {
                Original = name,
                Lower = name.ToLowerInvariant()
            })
            .ToList();

        var lowerNames = normalizedLookup
            .Select(entry => entry.Lower)
            .ToList();

        var existingCategories = await _context.Categories
            .Where(category => lowerNames.Contains(category.Name.ToLower()))
            .ToListAsync(cancellationToken);

        foreach (var entry in normalizedLookup)
        {
            if (existingCategories.Any(category => string.Equals(category.Name, entry.Original, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                Name = entry.Original
            };

            existingCategories.Add(category);
            _context.Categories.Add(category);
        }

        return normalizedLookup
            .Select(entry => existingCategories.First(category => string.Equals(category.Name, entry.Original, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }
}





