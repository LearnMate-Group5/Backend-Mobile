using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        _logger = logger;
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));

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
        if (string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            const string message = "AI webhook endpoint is not configured.";
            _logger.LogError(message);
            return new AiWebhookClientResponse(HttpStatusCode.InternalServerError, message, false);
        }

        Uri requestUri;
        try
        {
            requestUri = CreateRequestUri(_options.Endpoint);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Invalid AI webhook endpoint configured: {Endpoint}",
                _options.Endpoint);

            return new AiWebhookClientResponse(
                HttpStatusCode.InternalServerError,
                "AI webhook endpoint is invalid.",
                false);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        using var form = CreateFormContent(fileStream, fileName, contentType, userId);

        request.Content = form;

        try
        {
            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            var responseBody = response.Content is null
                ? string.Empty
                : await response.Content.ReadAsStringAsync(cancellationToken);

            return new AiWebhookClientResponse(
                response.StatusCode,
                responseBody,
                response.IsSuccessStatusCode);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "Timed out calling AI webhook at {Endpoint} for user {UserId}.",
                requestUri,
                userId);

            return new AiWebhookClientResponse(
                HttpStatusCode.RequestTimeout,
                "AI webhook call timed out.",
                false);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unexpected error calling AI webhook at {Endpoint} for user {UserId}.",
                requestUri,
                userId);

            return new AiWebhookClientResponse(
                HttpStatusCode.InternalServerError,
                exception.Message,
                false);
        }
    }

    private static Uri CreateRequestUri(string endpoint)
    {
        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var absolute))
        {
            return absolute;
        }

        var endpointWithScheme = endpoint.Contains("://", StringComparison.Ordinal)
            ? endpoint
            : $"http://{endpoint}";

        if (Uri.TryCreate(endpointWithScheme, UriKind.Absolute, out var absoluteWithScheme))
        {
            return absoluteWithScheme;
        }

        throw new InvalidOperationException($"Unable to create URI from endpoint value '{endpoint}'.");
    }

    private static MultipartFormDataContent CreateFormContent(
        Stream fileStream,
        string fileName,
        string contentType,
        string userId)
    {
        var form = new MultipartFormDataContent();

        var normalizedFileName = string.IsNullOrWhiteSpace(fileName)
            ? "upload"
            : Path.GetFileName(fileName);

        var streamContent = new StreamContent(fileStream);

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        }

        form.Add(streamContent, "File", normalizedFileName);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var userIdContent = new StringContent(userId, Encoding.UTF8);
            userIdContent.Headers.ContentType = null;
            form.Add(userIdContent, "userId");
        }

        return form;
    }
}
