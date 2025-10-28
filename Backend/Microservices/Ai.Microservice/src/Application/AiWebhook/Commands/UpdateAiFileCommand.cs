using Application.AiWebhook.Queries;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.AiWebhook.Commands;

public sealed record UpdateAiFileCommand(
    Guid FileId,
    string? FileName,
    string? OcrContent,
    string? TranslatedContent,
    string? CurrentContent) : IRequest<Result<AiFileDto>>;
