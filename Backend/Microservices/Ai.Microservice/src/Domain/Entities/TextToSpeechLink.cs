using System;

namespace Domain.Entities;

public class TextToSpeechLink
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string UniqueId { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
