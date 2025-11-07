using System;

namespace Application.Payments.DTOs
{
    public class PaymentHistoryResponse
    {
        public Guid Id { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string PaymentGateway { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string OrderInfo { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        // Gateway-specific transaction IDs
        public string? TransactionId { get; set; }
        public string? PaymentUrl { get; set; }
    }
}
