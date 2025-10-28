using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.AiWebhook.Commands;

public sealed record DeleteAiFileCommand(Guid FileId) : IRequest<Result>;
