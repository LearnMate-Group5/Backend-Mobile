using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class Subscription
{
    public Guid SubscriptionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal OriginalPrice { get; set; }

    public decimal Discount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
