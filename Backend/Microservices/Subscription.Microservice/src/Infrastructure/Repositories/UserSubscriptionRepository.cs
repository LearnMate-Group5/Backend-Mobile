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

public class UserSubscriptionRepository : IUserSubscriptionRepository
{
    private readonly MyDbContext _context;

    public UserSubscriptionRepository(MyDbContext context)
    {
        _context = context;
    }

    public async Task<UserSubscription?> GetByIdAsync(Guid userSubscriptionId, CancellationToken cancellationToken)
    {
        return await _context.UserSubscriptions
            .Include(us => us.Subscription)
            .FirstOrDefaultAsync(us => us.UserSubscriptionId == userSubscriptionId, cancellationToken);
    }

    public async Task<UserSubscription?> GetActiveByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await _context.UserSubscriptions
            .Include(us => us.Subscription)
            .Where(us => us.UserId == userId && (us.Status == "Current" || us.Status == "Cancelled"))
            .OrderByDescending(us => us.SubscribedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserSubscription>> GetByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await _context.UserSubscriptions
            .Include(us => us.Subscription)
            .Where(us => us.UserId == userId)
            .OrderByDescending(us => us.SubscribedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserHasActiveSubscriptionAsync(string userId, CancellationToken cancellationToken)
    {
        return await _context.UserSubscriptions
            .AnyAsync(us => us.UserId == userId && (us.Status == "Current" || us.Status == "Cancelled"), cancellationToken);
    }

    public async Task CreateAsync(UserSubscription userSubscription, CancellationToken cancellationToken)
    {
        await _context.UserSubscriptions.AddAsync(userSubscription, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserSubscription userSubscription, CancellationToken cancellationToken)
    {
        _context.UserSubscriptions.Update(userSubscription);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateUserSubscriptionsAsync(string userId, CancellationToken cancellationToken)
    {
        var activeSubscriptions = await _context.UserSubscriptions
            .Where(us => us.UserId == userId && (us.Status == "Current" || us.Status == "Cancelled"))
            .ToListAsync(cancellationToken);

        foreach (var subscription in activeSubscriptions)
        {
            subscription.Status = "Inactive";
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(UserSubscription userSubscription, CancellationToken cancellationToken)
    {
        _context.UserSubscriptions.Remove(userSubscription);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
