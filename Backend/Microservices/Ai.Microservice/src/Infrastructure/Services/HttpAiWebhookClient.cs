using System.Net.Http.Headers;
using Application.Common.Interfaces;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class HttpAiWebhookClient : IAiWebhookClient
{
    private readonly HttpClient _httpClient;
    private readonly AiWebhookOptions _options;
    private readonly ILogger<HttpAiWebhookClient> _logger;

    public HttpAiWebhookClient(
        HttpClient httpClient,
        IOptions<AiWebhookOptions> options,
        ILogger<HttpAiWebhookClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (_options.TimeoutSeconds > 0)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        }
    }

    public async Task<AiWebhookClientResponse> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string userId,
        CancellationToken cancellationToken)
    {
        var endpoint = _options.Endpoint;
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            _logger.LogError("AI webhook endpoint is not configured.");
            throw new InvalidOperationException("AI webhook endpoint is not configured.");
        }

        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        using var formContent = new MultipartFormDataContent();

        var mediaType = string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType;

        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        formContent.Add(streamContent, "File", fileName);

        formContent.Add(new StringContent(userId), "userId");

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = formContent
        };

        try
        {
            var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseContentRead,
                cancellationToken);

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);

            return new AiWebhookClientResponse(
                response.StatusCode,
                payload,
                response.IsSuccessStatusCode);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("AI webhook upload was cancelled for user {UserId}.", userId);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unexpected error while calling AI webhook for user {UserId}.",
                userId);
            throw;
        }
    }
}
