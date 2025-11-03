using System;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Commands;

public sealed record UpdateUserSubscriptionPlanCommand(
    Guid SubscriptionId,
    string UserId) : IRequest<Result<UserSubscriptionDto>>;

