using System.Net;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.AiWebhook.Commands;

public sealed record UploadAndTranslateCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    string UserId) : IRequest<Result<AiWebhookResponse>>;

public sealed record AiWebhookResponse(
    HttpStatusCode StatusCode,
    string Content,
    bool Success);

