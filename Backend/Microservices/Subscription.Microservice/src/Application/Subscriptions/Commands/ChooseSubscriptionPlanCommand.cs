using System;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Commands;

public sealed record ChooseSubscriptionPlanCommand(
    Guid SubscriptionId,
    string UserId) : IRequest<Result<UserSubscriptionDto>>;
