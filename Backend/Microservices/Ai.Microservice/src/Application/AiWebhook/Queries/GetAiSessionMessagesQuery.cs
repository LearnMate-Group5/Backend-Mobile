using SharedLibrary.Abstractions.Messaging;

namespace Application.AiWebhook.Queries;

public sealed record GetAiSessionMessagesQuery(
    string SessionId,
    string RequestingUserId,
    bool IncludeAll) : IQuery<AiSessionMessagesDto>;
