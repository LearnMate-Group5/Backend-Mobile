using System.Net;

namespace Application.Common.Interfaces;

public interface IAiWebhookClient
{
    Task<AiWebhookClientResponse> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string userId,
        CancellationToken cancellationToken);
}

public sealed record AiWebhookClientResponse(HttpStatusCode StatusCode, string Content, bool IsSuccess);

