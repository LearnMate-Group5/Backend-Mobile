using System;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Queries;

public sealed record GetSubscriptionPlanByIdQuery(Guid SubscriptionId) : IRequest<Result<SubscriptionDto>>;
