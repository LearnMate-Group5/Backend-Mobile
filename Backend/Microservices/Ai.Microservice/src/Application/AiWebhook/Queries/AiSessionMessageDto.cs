namespace Application.AiWebhook.Queries;

public sealed record AiSessionMessageDto(
    int Id,
    string SessionId,
    string Message);
