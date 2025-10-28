using System;

namespace Domain.Entities;

public class AiSession
{
    public string SessionId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string? Title { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? LastActivityDate { get; set; }
}
