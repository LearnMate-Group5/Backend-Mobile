using System;

namespace Domain.Entities;

public class UserSubscription
{
    public Guid UserSubscriptionId { get; set; }

    public Guid SubscriptionId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Status { get; set; } = "Current";

    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiredAt { get; set; }

    public Subscription? Subscription { get; set; }
}
