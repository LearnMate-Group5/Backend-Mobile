using Domain.Entities;

namespace Application.AiWebhook.Queries;

public sealed record AiFileDetailDto(
    Guid FileId,
    string FileName,
    string? OcrContent,
    string? TranslatedContent,
    string UserId,
    DateTime CreatedDate,
    AiFileStatus Status,
    string? CurrentContent);
