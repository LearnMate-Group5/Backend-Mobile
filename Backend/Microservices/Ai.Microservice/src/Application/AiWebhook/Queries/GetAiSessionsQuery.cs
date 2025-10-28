using System.Collections.Generic;
using SharedLibrary.Abstractions.Messaging;

namespace Application.AiWebhook.Queries;

public sealed record GetAiSessionsQuery(string UserId, bool IncludeAll) : IQuery<IReadOnlyList<AiSessionDto>>;
