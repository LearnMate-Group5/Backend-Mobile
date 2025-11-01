using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IBookRepository
{
    Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken);
    Task<Book?> GetByIdAsync(Guid bookId, CancellationToken cancellationToken);
    Task CreateAsync(Book book, IReadOnlyCollection<string> categoryNames, CancellationToken cancellationToken);
    Task UpdateAsync(Book book, IReadOnlyCollection<string>? categoryNames, CancellationToken cancellationToken);
    Task DeleteAsync(Book book, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid bookId, CancellationToken cancellationToken);
    Task<bool> TitleExistsAsync(string title, Guid? excludingBookId, CancellationToken cancellationToken);
    Task AddViewAsync(BookView view, CancellationToken cancellationToken);
    Task<bool> HasUserViewedAsync(Guid bookId, string viewerId, CancellationToken cancellationToken);
    Task<bool> HasUserLikedAsync(Guid bookId, string userId, CancellationToken cancellationToken);
    Task AddLikeAsync(BookLike like, CancellationToken cancellationToken);
    Task<int> GetViewCountAsync(Guid bookId, CancellationToken cancellationToken);
    Task<int> GetLikeCountAsync(Guid bookId, CancellationToken cancellationToken);
}
