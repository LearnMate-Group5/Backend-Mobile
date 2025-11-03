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

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly MyDbContext _context;

    public SubscriptionRepository(MyDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Subscription>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Subscriptions
            .AsNoTracking()
            .OrderBy(subscription => subscription.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Subscription?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken)
    {
        return await _context.Subscriptions
            .Include(subscription => subscription.UserSubscriptions)
            .FirstOrDefaultAsync(subscription => subscription.SubscriptionId == subscriptionId, cancellationToken);
    }

    public async Task<Subscription?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        var normalized = name.Trim();

        return await _context.Subscriptions
            .FirstOrDefaultAsync(
                subscription => EF.Functions.ILike(subscription.Name, normalized),
                cancellationToken);
    }

    public async Task<bool> NameAndTypeExistsAsync(string name, string type, Guid? excludingSubscriptionId, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim();
        var normalizedType = type.Trim();

        return await _context.Subscriptions.AnyAsync(
            subscription => EF.Functions.ILike(subscription.Name, normalizedName) &&
                            EF.Functions.ILike(subscription.Type, normalizedType) &&
                            (!excludingSubscriptionId.HasValue ||
                             subscription.SubscriptionId != excludingSubscriptionId.Value),
            cancellationToken);
    }

    public async Task CreateAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        await _context.Subscriptions.AddAsync(subscription, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        _context.Subscriptions.Remove(subscription);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
