using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IAiFileRepository
{
    Task SaveAsync(AiFile file, CancellationToken cancellationToken);
    Task<IReadOnlyList<AiFile>> GetAsync(bool includeAll, string userId, CancellationToken cancellationToken);
    Task<AiFile?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken);
    Task UpdateAsync(AiFile file, CancellationToken cancellationToken);
    Task DeleteAsync(Guid fileId, CancellationToken cancellationToken);
}
