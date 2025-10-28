using Domain.Entities;

namespace Application.AiWebhook.Queries;

public sealed record AiFileDto(
    Guid FileId,
    string FileName,
    string UserId,
    DateTime CreatedDate,
    AiFileStatus Status);
