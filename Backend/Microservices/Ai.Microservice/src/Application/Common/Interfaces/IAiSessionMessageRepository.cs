using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces;

public interface IAiSessionMessageRepository
{
    Task<IReadOnlyList<(string SessionId, int MessageCount)>> GetSessionSummariesAsync(IEnumerable<string> sessionIds, CancellationToken cancellationToken);
}
