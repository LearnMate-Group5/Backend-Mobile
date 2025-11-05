using System;

namespace SharedLibrary.Contracts;

/// <summary>
/// Request to get subscription price by UserSubscriptionId
/// </summary>
public class GetSubscriptionPriceRequest
{
    public Guid UserSubscriptionId { get; set; }
}
