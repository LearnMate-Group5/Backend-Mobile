using Application.AiWebhook.Commands;
using Application.AiWebhook.Queries;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.AiWebhook.Queries;

public sealed record GetAiFileByIdQuery(Guid FileId) : IRequest<Result<AiFileDetailDto>>;
