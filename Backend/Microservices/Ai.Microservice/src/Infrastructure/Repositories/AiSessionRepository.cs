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

public class AiSessionRepository : IAiSessionRepository
{
    private readonly MyDbContext _context;

    public AiSessionRepository(MyDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AiSession>> GetSessionsAsync(string userId, bool includeAll, CancellationToken cancellationToken)
    {
        var query = _context.AiSessions.AsNoTracking();

        if (!includeAll)
        {
            query = query.Where(session => session.UserId == userId);
        }

        return await query
            .OrderByDescending(session => session.LastActivityDate ?? session.CreatedDate)
            .ToListAsync(cancellationToken);
    }
}
