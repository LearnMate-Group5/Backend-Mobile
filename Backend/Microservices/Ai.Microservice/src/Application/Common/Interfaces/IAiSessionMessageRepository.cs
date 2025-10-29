using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IAiSessionMessageRepository
{
    Task<IReadOnlyList<(string SessionId, int MessageCount)>> GetSessionSummariesAsync(IEnumerable<string> sessionIds, CancellationToken cancellationToken);

    Task<IReadOnlyList<AiSessionMessage>> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken);
}
