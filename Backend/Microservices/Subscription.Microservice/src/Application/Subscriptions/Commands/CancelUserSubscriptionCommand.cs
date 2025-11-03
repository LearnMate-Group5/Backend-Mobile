using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Commands;

public sealed record CancelUserSubscriptionCommand(
    string UserId) : IRequest<Result<UserSubscriptionDto>>;
