using System;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Commands;

public sealed record DeleteSubscriptionPlanCommand(Guid SubscriptionId) : IRequest<Result>;
