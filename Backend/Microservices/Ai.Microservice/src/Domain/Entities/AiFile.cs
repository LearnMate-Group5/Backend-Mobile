using System;

namespace Domain.Entities;

public partial class AiFile
{
    public Guid FileId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string? OcrContent { get; set; }

    public string? TranslatedContent { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public AiFileStatus Status { get; set; } = AiFileStatus.Active;

    public string? CurrentContent { get; set; }
}

public enum AiFileStatus
{
    Deleted = 0,
    Active = 1
}
