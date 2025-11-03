using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IUserSubscriptionRepository
{
    Task<UserSubscription?> GetByIdAsync(Guid userSubscriptionId, CancellationToken cancellationToken);
    Task<UserSubscription?> GetActiveByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserSubscription>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<bool> UserHasActiveSubscriptionAsync(string userId, CancellationToken cancellationToken);
    Task CreateAsync(UserSubscription userSubscription, CancellationToken cancellationToken);
    Task UpdateAsync(UserSubscription userSubscription, CancellationToken cancellationToken);
    Task DeactivateUserSubscriptionsAsync(string userId, CancellationToken cancellationToken);
    Task DeleteAsync(UserSubscription userSubscription, CancellationToken cancellationToken);
}
