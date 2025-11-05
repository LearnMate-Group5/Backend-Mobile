using Application.Payments.DTOs;
using Application.Payments.Services;
using Microsoft.Extensions.Logging;
using SharedLibrary.Abstractions.Messaging;
using SharedLibrary.Common.ResponseModel;

namespace Application.Payments.Commands;

public class CreateMoMoOrderCommandHandler : ICommandHandler<CreateMoMoOrderCommand, CreateMoMoOrderResponse>
{
    private readonly IMoMoService _momoService;
    private readonly ILogger<CreateMoMoOrderCommandHandler> _logger;

    public CreateMoMoOrderCommandHandler(
        IMoMoService momoService,
        ILogger<CreateMoMoOrderCommandHandler> logger)
    {
        _momoService = momoService;
        _logger = logger;
    }

    public async Task<Result<CreateMoMoOrderResponse>> Handle(
        CreateMoMoOrderCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating MoMo order for user: {UserId}, orderId: {OrderId}",
                request.UserId, request.OrderId);

            // Map command to service DTO
            var serviceRequest = new CreateMoMoOrderRequest
            {
                OrderInfo = request.OrderInfo,
                RedirectUrl = request.RedirectUrl,
                ExtraData = request.ExtraData,
                Lang = request.Lang,
                OrderId = request.OrderId
            };

            var serviceResult = await _momoService.CreateOrderAsync(request.UserId, serviceRequest);

            if (!serviceResult.Success)
            {
                _logger.LogWarning("Failed to create MoMo order: {Message}", serviceResult.Message);

                // Include subErrors if available for detailed debugging
                var errorMessage = serviceResult.Message;
                if (serviceResult.SubErrors != null && serviceResult.SubErrors.Count > 0)
                {
                    var subErrorsDescription = string.Join("; ",
                        serviceResult.SubErrors.Select(e => $"{e.Field}: {e.Message}"));
                    errorMessage = $"{serviceResult.Message}. Details: {subErrorsDescription}";
                }

                return Result.Failure<CreateMoMoOrderResponse>(
                    new Error("MoMo.CreateOrderFailed", errorMessage));
            }

            // Map service result to command response
            var response = new CreateMoMoOrderResponse(
                PayUrl: serviceResult.PayUrl ?? "",
                Deeplink: serviceResult.Deeplink ?? "",
                QrCodeUrl: serviceResult.QrCodeUrl ?? "",
                OrderId: serviceResult.OrderId ?? "",
                RequestId: serviceResult.RequestId ?? ""
            );

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateMoMoOrderCommandHandler");
            return Result.Failure<CreateMoMoOrderResponse>(
                new Error("Payment.SystemError", "Internal server error"));
        }
    }
}
