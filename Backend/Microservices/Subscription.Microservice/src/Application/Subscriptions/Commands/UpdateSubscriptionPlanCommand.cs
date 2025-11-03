using System;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Commands;

public sealed record UpdateSubscriptionPlanCommand(
    Guid SubscriptionId,
    string? Name,
    string? Type,
    string? Status,
    decimal? OriginalPrice,
    decimal? Discount) : IRequest<Result<SubscriptionDto>>;
