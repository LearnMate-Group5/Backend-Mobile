using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IAiFileRepository
{
    Task SaveAsync(AiFile file, CancellationToken cancellationToken);
}
