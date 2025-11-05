using System;
using System.Threading.Tasks;
using Application.Payments.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedLibrary.Contracts;

namespace Infrastructure.Services;

/// <summary>
/// Service for communicating with Subscription microservice via RabbitMQ
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly IRequestClient<GetSubscriptionPriceRequest> _requestClient;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        IRequestClient<GetSubscriptionPriceRequest> requestClient,
        ILogger<SubscriptionService> logger)
    {
        _requestClient = requestClient;
        _logger = logger;
    }

    public async Task<GetSubscriptionPriceResponse> GetSubscriptionPriceAsync(Guid userSubscriptionId)
    {
        try
        {
            _logger.LogInformation("Requesting subscription price for UserSubscriptionId: {UserSubscriptionId}",
                userSubscriptionId);

            var request = new GetSubscriptionPriceRequest
            {
                UserSubscriptionId = userSubscriptionId
            };

            // Send request and wait for response (with timeout)
            var response = await _requestClient.GetResponse<GetSubscriptionPriceResponse>(
                request,
                timeout: RequestTimeout.After(s: 10));

            var priceInfo = response.Message;

            _logger.LogInformation(
                "Received subscription price response: Success={Success}, FinalPrice={FinalPrice}",
                priceInfo.Success,
                priceInfo.FinalPrice);

            return priceInfo;
        }
        catch (RequestTimeoutException ex)
        {
            _logger.LogError(ex, "Timeout waiting for subscription price response for UserSubscriptionId: {UserSubscriptionId}",
                userSubscriptionId);

            return new GetSubscriptionPriceResponse
            {
                Success = false,
                Message = "Timeout: Subscription service did not respond in time",
                UserSubscriptionId = userSubscriptionId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription price for UserSubscriptionId: {UserSubscriptionId}",
                userSubscriptionId);

            return new GetSubscriptionPriceResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}",
                UserSubscriptionId = userSubscriptionId
            };
        }
    }
}
