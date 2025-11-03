using System;
using Domain.Entities;

namespace Application.Subscriptions;

public sealed record SubscriptionDto(
    Guid SubscriptionId,
    string Name,
    string Type,
    string Status,
    decimal OriginalPrice,
    decimal Discount)
{
    public static SubscriptionDto FromEntity(Subscription subscription) =>
        new(
            subscription.SubscriptionId,
            subscription.Name,
            subscription.Type,
            subscription.Status,
            subscription.OriginalPrice,
            subscription.Discount);
}
