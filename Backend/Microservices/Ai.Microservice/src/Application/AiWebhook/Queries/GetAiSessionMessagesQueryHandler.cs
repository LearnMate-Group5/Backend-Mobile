using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Abstractions.Messaging;

namespace Application.AiWebhook.Queries;

internal sealed class GetAiSessionMessagesQueryHandler
    : IQueryHandler<GetAiSessionMessagesQuery, AiSessionMessagesDto>
{
    private readonly IAiSessionRepository _sessionRepository;
    private readonly IAiSessionMessageRepository _messageRepository;

    public GetAiSessionMessagesQueryHandler(
        IAiSessionRepository sessionRepository,
        IAiSessionMessageRepository messageRepository)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
    }

    public async Task<Result<AiSessionMessagesDto>> Handle(
        GetAiSessionMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null)
        {
            return Result.Failure<AiSessionMessagesDto>(
                new Error("AiSession.NotFound", $"Session with id '{request.SessionId}' was not found."));
        }

        if (!request.IncludeAll &&
            !session.UserId.Equals(request.RequestingUserId, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<AiSessionMessagesDto>(
                new Error("AiSession.AccessDenied", "You do not have permission to access this session."));
        }

        var messages = await _messageRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);
        var dto = new AiSessionMessagesDto(
            session.SessionId,
            session.UserId,
            messages
                .Select(message => new AiSessionMessageDto(
                    message.Id,
                    message.SessionId,
                    message.Message))
                .ToList()
                .AsReadOnly());

        return Result.Success(dto);
    }
}
