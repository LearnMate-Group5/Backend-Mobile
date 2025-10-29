using System.Collections.Generic;

namespace Application.AiWebhook.Queries;

public sealed record AiSessionMessagesDto(
    string SessionId,
    string UserId,
    IReadOnlyList<AiSessionMessageDto> Messages);
