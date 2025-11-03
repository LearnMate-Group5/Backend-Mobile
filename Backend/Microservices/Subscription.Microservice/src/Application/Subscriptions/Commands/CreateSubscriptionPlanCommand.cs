using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Commands;

public sealed record CreateSubscriptionPlanCommand(
    string Name,
    string Type,
    string Status,
    decimal OriginalPrice,
    decimal Discount) : IRequest<Result<SubscriptionDto>>;
