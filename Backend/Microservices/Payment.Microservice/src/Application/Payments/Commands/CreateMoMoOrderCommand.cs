using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.Commands;

public record CreateMoMoOrderCommand(
    string UserId,
    string OrderId, // Required: UserSubscriptionId (Guid) - used to fetch the subscription price
    string OrderInfo,
    string? RedirectUrl = null,
    string? ExtraData = null,
    string Lang = "vi"
) : ICommand<CreateMoMoOrderResponse>;

public record CreateMoMoOrderResponse(
    string PayUrl,
    string Deeplink,
    string QrCodeUrl,
    string OrderId,
    string RequestId
);
