using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AiSessionMessageRepository : IAiSessionMessageRepository
{
    private readonly MyDbContext _context;

    public AiSessionMessageRepository(MyDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<(string SessionId, int MessageCount)>> GetSessionSummariesAsync(IEnumerable<string> sessionIds, CancellationToken cancellationToken)
    {
        var idList = sessionIds?.ToArray() ?? Array.Empty<string>();
        if (idList.Length == 0)
        {
            return Array.Empty<(string SessionId, int MessageCount)>();
        }

        return await _context.AiSessionMessages
            .AsNoTracking()
            .Where(message => idList.Contains(message.SessionId))
            .GroupBy(message => message.SessionId)
            .Select(group => new ValueTuple<string, int>(group.Key, group.Count()))
            .OrderByDescending(tuple => tuple.Item2)
            .ToListAsync(cancellationToken);
    }
}
