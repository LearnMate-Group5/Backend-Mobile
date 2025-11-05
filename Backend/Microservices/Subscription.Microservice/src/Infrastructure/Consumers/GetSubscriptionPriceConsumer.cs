using System;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedLibrary.Contracts;

namespace Infrastructure.Consumers;

/// <summary>
/// Consumer that handles requests to get subscription price information
/// </summary>
public class GetSubscriptionPriceConsumer : IConsumer<GetSubscriptionPriceRequest>
{
    private readonly IUserSubscriptionRepository _userSubscriptionRepository;
    private readonly ILogger<GetSubscriptionPriceConsumer> _logger;

    public GetSubscriptionPriceConsumer(
        IUserSubscriptionRepository userSubscriptionRepository,
        ILogger<GetSubscriptionPriceConsumer> logger)
    {
        _userSubscriptionRepository = userSubscriptionRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GetSubscriptionPriceRequest> context)
    {
        var request = context.Message;

        _logger.LogInformation("Received request to get price for UserSubscriptionId: {UserSubscriptionId}",
            request.UserSubscriptionId);

        try
        {
            // Get user subscription with subscription details
            var userSubscription = await _userSubscriptionRepository.GetByIdAsync(
                request.UserSubscriptionId,
                context.CancellationToken);

            if (userSubscription == null || userSubscription.Subscription == null)
            {
                _logger.LogWarning("UserSubscription not found: {UserSubscriptionId}", request.UserSubscriptionId);

                await context.RespondAsync(new GetSubscriptionPriceResponse
                {
                    Success = false,
                    Message = $"User subscription {request.UserSubscriptionId} not found",
                    UserSubscriptionId = request.UserSubscriptionId
                });
                return;
            }

            var subscription = userSubscription.Subscription;

            // Calculate final price: OriginalPrice - (OriginalPrice * Discount / 100)
            var finalPrice = subscription.OriginalPrice - (subscription.OriginalPrice * subscription.Discount / 100);

            _logger.LogInformation(
                "Found subscription price for UserSubscriptionId: {UserSubscriptionId}, " +
                "SubscriptionId: {SubscriptionId}, OriginalPrice: {OriginalPrice}, " +
                "Discount: {Discount}%, FinalPrice: {FinalPrice}",
                request.UserSubscriptionId,
                subscription.SubscriptionId,
                subscription.OriginalPrice,
                subscription.Discount,
                finalPrice);

            await context.RespondAsync(new GetSubscriptionPriceResponse
            {
                Success = true,
                Message = "Price retrieved successfully",
                UserSubscriptionId = userSubscription.UserSubscriptionId,
                SubscriptionId = subscription.SubscriptionId,
                OriginalPrice = subscription.OriginalPrice,
                Discount = subscription.Discount,
                FinalPrice = finalPrice,
                SubscriptionName = subscription.Name,
                SubscriptionType = subscription.Type
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscription price for UserSubscriptionId: {UserSubscriptionId}",
                request.UserSubscriptionId);

            await context.RespondAsync(new GetSubscriptionPriceResponse
            {
                Success = false,
                Message = $"Error retrieving price: {ex.Message}",
                UserSubscriptionId = request.UserSubscriptionId
            });
        }
    }
}
