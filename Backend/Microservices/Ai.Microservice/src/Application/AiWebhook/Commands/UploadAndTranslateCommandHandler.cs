using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedLibrary.Common.ResponseModel;
using System.Text.Json;

namespace Application.AiWebhook.Commands;

public class UploadAndTranslateCommandHandler
    : IRequestHandler<UploadAndTranslateCommand, Result<AiWebhookResponse>>
{
    private readonly IAiWebhookClient _aiWebhookClient;
    private readonly IAiFileRepository _aiFileRepository;
    private readonly ILogger<UploadAndTranslateCommandHandler> _logger;

    public UploadAndTranslateCommandHandler(
        IAiWebhookClient aiWebhookClient,
        IAiFileRepository aiFileRepository,
        ILogger<UploadAndTranslateCommandHandler> logger)
    {
        _aiWebhookClient = aiWebhookClient;
        _aiFileRepository = aiFileRepository;
        _logger = logger;
    }

    public async Task<Result<AiWebhookResponse>> Handle(
        UploadAndTranslateCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.FileStream.CanSeek)
            {
                request.FileStream.Position = 0;
            }

            var response = await _aiWebhookClient.UploadAsync(
                request.FileStream,
                request.FileName,
                request.ContentType,
                request.UserId,
                cancellationToken);

            if (!response.IsSuccess)
            {
                var description = string.IsNullOrWhiteSpace(response.Content)
                    ? $"Webhook call failed with status code {(int)response.StatusCode}."
                    : response.Content;

                return Result.Failure<AiWebhookResponse>(
                    new Error("AiWebhook.UploadFailed", description));
            }

            await PersistResponseAsync(response.Content, request, cancellationToken);

            var result = new AiWebhookResponse(
                response.StatusCode,
                response.Content,
                Success: true);

            return Result.Success(result);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to upload file for user {UserId} through AI webhook.",
                request.UserId);

            return Result.Failure<AiWebhookResponse>(Error.FromException(exception));
        }
    }

    private async Task PersistResponseAsync(
        string? responseContent,
        UploadAndTranslateCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(responseContent);
            if (document.RootElement.ValueKind != JsonValueKind.Array ||
                document.RootElement.GetArrayLength() == 0)
            {
                return;
            }

            var first = document.RootElement[0];

            var trans = TryGetString(first, "trans");
            var markdown = TryGetString(first, "markdown");
            var fileName = TryGetString(first, "filename") ?? request.FileName;
            var fileId = TryGetGuid(first, "id") ?? Guid.NewGuid();
            var createdDate = TryGetCreatedDate(first) ?? DateTime.UtcNow;

            string? pageContent = TryGetString(first, "pageContent");
            if (string.IsNullOrWhiteSpace(pageContent) &&
                document.RootElement.GetArrayLength() > 1)
            {
                var second = document.RootElement[1];
                pageContent = TryGetString(second, "pageContent");
            }

            var aiFile = new AiFile
            {
                FileId = fileId,
                FileName = fileName,
                OcrContent = markdown,
                TranslatedContent = trans,
                UserId = request.UserId,
                CreatedDate = createdDate,
                Status = AiFileStatus.Active,
                CurrentContent = pageContent ?? trans ?? markdown ?? responseContent
            };

            await _aiFileRepository.SaveAsync(aiFile, cancellationToken);
        }
        catch (JsonException jsonException)
        {
            _logger.LogError(
                jsonException,
                "Failed to parse AI webhook response for persistence. User {UserId}",
                request.UserId);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to persist AI webhook response for user {UserId}",
                request.UserId);
        }
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind != JsonValueKind.Null
            ? property.GetString()
            : null;
    }

    private static Guid? TryGetGuid(JsonElement element, string propertyName)
    {
        var value = TryGetString(element, propertyName);
        if (Guid.TryParse(value, out var guid))
        {
            return guid;
        }

        return null;
    }

    private static DateTime? TryGetCreatedDate(JsonElement element)
    {
        if (!element.TryGetProperty("created_at", out var createdAt))
        {
            return null;
        }

        if (createdAt.ValueKind == JsonValueKind.Number && createdAt.TryGetInt64(out var unixSeconds))
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;
        }

        if (createdAt.ValueKind == JsonValueKind.String &&
            DateTime.TryParse(createdAt.GetString(), out var parsedDate))
        {
            return parsedDate;
        }

        return null;
    }
}

