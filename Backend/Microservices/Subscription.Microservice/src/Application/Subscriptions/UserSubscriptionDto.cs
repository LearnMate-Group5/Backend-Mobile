using System;
using Domain.Entities;

namespace Application.Subscriptions;

public sealed record UserSubscriptionDto(
    Guid UserSubscriptionId,
    Guid SubscriptionId,
    string UserId,
    string Status,
    DateTime SubscribedAt,
    DateTime? ExpiredAt,
    string Name,
    string Type,
    string SubscriptionStatus,
    decimal OriginalPrice,
    decimal Discount)
{
    public static UserSubscriptionDto FromEntity(UserSubscription userSubscription) =>
        new(
            userSubscription.UserSubscriptionId,
            userSubscription.SubscriptionId,
            userSubscription.UserId,
            userSubscription.Status,
            userSubscription.SubscribedAt,
            userSubscription.ExpiredAt,
            userSubscription.Subscription?.Name ?? string.Empty,
            userSubscription.Subscription?.Type ?? string.Empty,
            userSubscription.Subscription?.Status ?? string.Empty,
            userSubscription.Subscription?.OriginalPrice ?? 0,
            userSubscription.Subscription?.Discount ?? 0);
}
