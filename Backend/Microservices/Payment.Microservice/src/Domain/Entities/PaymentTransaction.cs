using System;

namespace Domain.Entities
{
    public class PaymentTransaction
    {
        public Guid Id { get; set; }
        
        // Common fields
        public string? UserId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string PaymentGateway { get; set; } = string.Empty; // "ZaloPay", "MoMo"
        public string Status { get; set; } = string.Empty; // "Pending", "Success", "Failed", "Expired"
        public string OrderInfo { get; set; } = string.Empty;
        public int ResultCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; } // Transaction expiration time (15 minutes from creation)
        
        // ZaloPay specific fields
        public string? AppTransId { get; set; }
        public string? ZpTransToken { get; set; }
        public string? QrCode { get; set; }
        
        // MoMo specific fields
        public string? MomoTransId { get; set; }
        public string? PayType { get; set; }
        public string? PayUrl { get; set; } // MoMo payment URL
        public string? Deeplink { get; set; } // MoMo deeplink
        public string? QrCodeUrl { get; set; } // MoMo QR code URL
        
        // Callback data
        public string? CallbackData { get; set; }
    }
}
