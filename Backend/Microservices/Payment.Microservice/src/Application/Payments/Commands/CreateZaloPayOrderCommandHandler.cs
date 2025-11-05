using Application.Payments.DTOs;
using Application.Payments.Services;
using Microsoft.Extensions.Logging;
using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.Commands;

public class CreateZaloPayOrderCommandHandler : ICommandHandler<CreateZaloPayOrderCommand, CreateZaloPayOrderResponse>
{
    private readonly IZaloPayService _zaloPayService;
    private readonly ILogger<CreateZaloPayOrderCommandHandler> _logger;

    public CreateZaloPayOrderCommandHandler(
        IZaloPayService zaloPayService,
        ILogger<CreateZaloPayOrderCommandHandler> logger)
    {
        _zaloPayService = zaloPayService;
        _logger = logger;
    }

    public async Task<Result<CreateZaloPayOrderResponse>> Handle(
        CreateZaloPayOrderCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating ZaloPay order for user: {UserId}, orderId: {OrderId}",
                request.UserId, request.OrderId);

            // Map command to service DTO
            var serviceRequest = new CreateZaloPayOrderRequest
            {
                Description = request.Description,
                RedirectUrl = request.RedirectUrl,
                OrderId = request.OrderId
            };

            var serviceResult = await _zaloPayService.CreateOrderAsync(request.UserId, serviceRequest);

            if (!serviceResult.Success)
            {
                _logger.LogWarning("Failed to create ZaloPay order: {Message}", serviceResult.Message);
                return Result.Failure<CreateZaloPayOrderResponse>(
                    new Error("ZaloPay.CreateOrderFailed", serviceResult.Message));
            }

            // Map service result to command response
            var response = new CreateZaloPayOrderResponse(
                OrderUrl: serviceResult.OrderUrl ?? "",
                AppTransId: serviceResult.AppTransId ?? "",
                ZpTransToken: serviceResult.ZpTransToken ?? "",
                OrderToken: serviceResult.OrderToken ?? "",
                QrCode: serviceResult.QrCode ?? ""
            );

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateZaloPayOrderCommandHandler");
            return Result.Failure<CreateZaloPayOrderResponse>(
                new Error("Payment.SystemError", "Internal server error"));
        }
    }
}
