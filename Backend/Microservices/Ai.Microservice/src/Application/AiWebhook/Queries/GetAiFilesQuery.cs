using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.AiWebhook.Queries;

public sealed record GetAiFilesQuery(
    string UserId,
    bool IncludeAll) : IRequest<Result<IReadOnlyList<AiFileDto>>>;
