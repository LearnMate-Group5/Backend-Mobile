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

public class CategoryRepository : ICategoryRepository
{
    private readonly MyDbContext _context;

    public CategoryRepository(MyDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .Include(category => category.BookCategories)
                .ThenInclude(bookCategory => bookCategory.Book)
            .FirstOrDefaultAsync(category => category.CategoryId == categoryId, cancellationToken);
    }

    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        var normalized = name.Trim();

        return await _context.Categories
            .FirstOrDefaultAsync(
                category => EF.Functions.ILike(category.Name, normalized),
                cancellationToken);
    }

    public async Task CreateAsync(Category category, CancellationToken cancellationToken)
    {
        await _context.Categories.AddAsync(category, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Category category, CancellationToken cancellationToken)
    {
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludingCategoryId, CancellationToken cancellationToken)
    {
        var normalized = name.Trim();

        return await _context.Categories.AnyAsync(
            category => EF.Functions.ILike(category.Name, normalized) &&
                        (!excludingCategoryId.HasValue || category.CategoryId != excludingCategoryId.Value),
            cancellationToken);
    }
}
