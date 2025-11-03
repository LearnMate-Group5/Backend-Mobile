using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Queries;

public sealed record GetActiveUserSubscriptionQuery(string UserId)
    : IRequest<Result<UserSubscriptionDto>>;

