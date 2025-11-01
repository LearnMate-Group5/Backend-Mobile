using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken);
    Task<Category?> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken);
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task CreateAsync(Category category, CancellationToken cancellationToken);
    Task UpdateAsync(Category category, CancellationToken cancellationToken);
    Task DeleteAsync(Category category, CancellationToken cancellationToken);
    Task<bool> NameExistsAsync(string name, Guid? excludingCategoryId, CancellationToken cancellationToken);
}
