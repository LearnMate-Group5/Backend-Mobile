using System.Collections.Generic;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Queries;

public sealed record GetSubscriptionPlansQuery : IRequest<Result<IReadOnlyList<SubscriptionDto>>>;
