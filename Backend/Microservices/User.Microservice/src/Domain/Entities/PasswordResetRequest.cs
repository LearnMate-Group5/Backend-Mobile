using System;

namespace Domain.Entities;

public class PasswordResetRequest
{
    public Guid PasswordResetRequestId { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
    public string OtpHash { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Used { get; set; }

    public virtual User User { get; set; } = null!;
}
