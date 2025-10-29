using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

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

        var groupedSummaries = await _context.AiSessionMessages
            .AsNoTracking()
            .Where(message => idList.Contains(message.SessionId))
            .GroupBy(message => message.SessionId)
            .Select(group => new
            {
                SessionId = group.Key,
                MessageCount = group.Count()
            })
            .ToListAsync(cancellationToken);

        return groupedSummaries
            .Select(summary => (summary.SessionId, summary.MessageCount))
            .OrderByDescending(tuple => tuple.MessageCount)
            .ToList();
    }

    public async Task<IReadOnlyList<AiSessionMessage>> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return Array.Empty<AiSessionMessage>();
        }

        return await _context.AiSessionMessages
            .AsNoTracking()
            .Where(message => message.SessionId == sessionId)
            .OrderBy(message => message.Id)
            .ToListAsync(cancellationToken);
    }
}
