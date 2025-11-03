using System.Collections.Generic;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Queries;

public sealed record GetUserSubscriptionsQuery(string UserId)
    : IRequest<Result<IReadOnlyList<UserSubscriptionDto>>>;

