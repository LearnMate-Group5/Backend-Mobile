using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Common;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SharedLibrary.Extensions;

namespace Infrastructure.Repositories;

public class PasswordResetRequestRepository : Repository<PasswordResetRequest>, IPasswordResetRequestRepository
{
    public PasswordResetRequestRepository(MyDbContext context) : base(context)
    {
    }

    public async Task InvalidateActiveRequestsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var requests = await _context.PasswordResetRequests
            .Where(r => r.UserId == userId && !r.Used && r.ExpiresAt > DateTimeExtensions.PostgreSqlUtcNow)
            .ToListAsync(cancellationToken);

        foreach (var request in requests)
        {
            request.Used = true;
        }

        if (requests.Count > 0)
        {
            _context.PasswordResetRequests.UpdateRange(requests);
        }
    }

    public Task<PasswordResetRequest?> GetActiveByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult<PasswordResetRequest?>(null);
        }

        var normalizedToken = token.Trim();
        var now = DateTimeExtensions.PostgreSqlUtcNow;

        return _context.PasswordResetRequests
            .Where(r => r.Token == normalizedToken && !r.Used && r.ExpiresAt > now)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<PasswordResetRequest?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeExtensions.PostgreSqlUtcNow;

        return _context.PasswordResetRequests
            .Where(r => r.UserId == userId && !r.Used && r.ExpiresAt > now)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
