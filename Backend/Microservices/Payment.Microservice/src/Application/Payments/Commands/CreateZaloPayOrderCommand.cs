using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.Commands;

public record CreateZaloPayOrderCommand(
    string UserId,
    string OrderId, // Required: UserSubscriptionId (Guid) - used to fetch the subscription price
    string Description,
    string? RedirectUrl = null
) : ICommand<CreateZaloPayOrderResponse>;

public record CreateZaloPayOrderResponse(
    string OrderUrl,
    string AppTransId,
    string ZpTransToken,
    string OrderToken,
    string QrCode
);
