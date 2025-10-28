using System;

namespace Application.AiWebhook.Queries;

public sealed record AiSessionDto(
    string SessionId,
    string UserId,
    string? Title,
    DateTime CreatedDate,
    DateTime? LastActivityDate,
    int MessageCount);
