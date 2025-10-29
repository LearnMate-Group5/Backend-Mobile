using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IAiSessionRepository
{
    Task<IReadOnlyList<AiSession>> GetSessionsAsync(string userId, bool includeAll, CancellationToken cancellationToken);

    Task<AiSession?> GetByIdAsync(string sessionId, CancellationToken cancellationToken);
}
