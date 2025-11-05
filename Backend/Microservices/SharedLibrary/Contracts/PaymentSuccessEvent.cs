namespace SharedLibrary.Contracts;

/// <summary>
/// Event published when a payment is successfully completed
/// </summary>
public sealed record PaymentSuccessEvent
{
    /// <summary>
    /// The order ID (UserSubscriptionId in subscription context)
    /// </summary>
    public required string OrderId { get; init; }

    /// <summary>
    /// The user ID who made the payment
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Payment method used (ZaloPay, MoMo, etc.)
    /// </summary>
    public required string PaymentMethod { get; init; }

    /// <summary>
    /// Payment amount
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Transaction ID from payment gateway
    /// </summary>
    public string? TransactionId { get; init; }

    /// <summary>
    /// Timestamp when payment was completed
    /// </summary>
    public DateTime CompletedAt { get; init; }
}
