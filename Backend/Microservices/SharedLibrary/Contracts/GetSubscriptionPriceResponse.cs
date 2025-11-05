using System;

namespace SharedLibrary.Contracts;

/// <summary>
/// Response containing subscription price information
/// </summary>
public class GetSubscriptionPriceResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid UserSubscriptionId { get; set; }
    public Guid SubscriptionId { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal FinalPrice { get; set; } // OriginalPrice - (OriginalPrice * Discount / 100)
    public string SubscriptionName { get; set; } = string.Empty;
    public string SubscriptionType { get; set; } = string.Empty;
}
