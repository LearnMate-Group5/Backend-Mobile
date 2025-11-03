using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ISubscriptionRepository
{
    Task<IReadOnlyList<Subscription>> GetAllAsync(CancellationToken cancellationToken);
    Task<Subscription?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken);
    Task<Subscription?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<bool> NameAndTypeExistsAsync(string name, string type, Guid? excludingSubscriptionId, CancellationToken cancellationToken);
    Task CreateAsync(Subscription subscription, CancellationToken cancellationToken);
    Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken);
    Task DeleteAsync(Subscription subscription, CancellationToken cancellationToken);
}
