using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;
using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Common.ResponseModel;

namespace Application.AiWebhook.Queries;

internal sealed class GetAiSessionsQueryHandler
    : IQueryHandler<GetAiSessionsQuery, IReadOnlyList<AiSessionDto>>
{
    private readonly IAiSessionRepository _sessionRepository;
    private readonly IAiSessionMessageRepository _messageRepository;

    public GetAiSessionsQueryHandler(
        IAiSessionRepository sessionRepository,
        IAiSessionMessageRepository messageRepository)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
    }

    public async Task<Result<IReadOnlyList<AiSessionDto>>> Handle(
        GetAiSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var sessions = await _sessionRepository.GetSessionsAsync(request.UserId, request.IncludeAll, cancellationToken);
        if (sessions.Count == 0)
        {
            return Result.Success<IReadOnlyList<AiSessionDto>>(Array.Empty<AiSessionDto>());
        }

        var sessionIds = sessions.Select(session => session.SessionId).ToArray();
        var summaries = await _messageRepository.GetSessionSummariesAsync(sessionIds, cancellationToken);
        var summaryLookup = summaries.ToDictionary(tuple => tuple.SessionId, tuple => tuple.MessageCount, StringComparer.Ordinal);

        var dtos = sessions
            .Select(session =>
            {
                summaryLookup.TryGetValue(session.SessionId, out var messageCount);

                return new AiSessionDto(
                    session.SessionId,
                    session.UserId,
                    session.Title,
                    session.CreatedDate,
                    session.LastActivityDate,
                    messageCount);
            })
            .OrderByDescending(dto => dto.LastActivityDate ?? dto.CreatedDate)
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<AiSessionDto>>(dtos);
    }
}
