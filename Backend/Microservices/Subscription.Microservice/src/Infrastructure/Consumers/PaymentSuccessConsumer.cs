using System;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedLibrary.Contracts;

namespace Infrastructure.Consumers;

public class PaymentSuccessConsumer : IConsumer<PaymentSuccessEvent>
{
    private readonly ILogger<PaymentSuccessConsumer> _logger;
    private readonly IUserSubscriptionRepository _userSubscriptionRepo;

    public PaymentSuccessConsumer(
        ILogger<PaymentSuccessConsumer> logger,
        IUserSubscriptionRepository userSubscriptionRepo)
    {
        _logger = logger;
        _userSubscriptionRepo = userSubscriptionRepo;
    }

    public async Task Consume(ConsumeContext<PaymentSuccessEvent> context)
    {
        var paymentEvent = context.Message;

        _logger.LogInformation("Received payment success event for OrderId: {OrderId}", paymentEvent.OrderId);

        try
        {
            // OrderId is actually UserSubscriptionId (Guid)
            if (!Guid.TryParse(paymentEvent.OrderId, out var userSubscriptionId))
            {
                _logger.LogWarning("Invalid OrderId format: {OrderId}. Expected a Guid.", paymentEvent.OrderId);
                return;
            }

            var userSubscription = await _userSubscriptionRepo.GetByIdAsync(userSubscriptionId, context.CancellationToken);

            if (userSubscription == null)
            {
                _logger.LogWarning("UserSubscription not found for OrderId: {OrderId}", paymentEvent.OrderId);
                return;
            }

            // Handle Pending status (new subscription)
            if (userSubscription.Status == "Pending")
            {
                userSubscription.Status = "Current";
                await _userSubscriptionRepo.UpdateAsync(userSubscription, context.CancellationToken);

                _logger.LogInformation(
                    "Updated UserSubscription {UserSubscriptionId} status from Pending to Current. UserId: {UserId}, PaymentMethod: {PaymentMethod}, Amount: {Amount}",
                    userSubscriptionId, paymentEvent.UserId, paymentEvent.PaymentMethod, paymentEvent.Amount);
            }
            // Handle PendingUpgrade status (upgrade subscription)
            else if (userSubscription.Status == "PendingUpgrade")
            {
                // Get the current active subscription for this user
                var currentSubscription = await _userSubscriptionRepo.GetActiveByUserIdAsync(
                    userSubscription.UserId,
                    context.CancellationToken);

                // Change old Current subscription to Active
                if (currentSubscription != null && currentSubscription.Status == "Current")
                {
                    currentSubscription.Status = "Active";
                    await _userSubscriptionRepo.UpdateAsync(currentSubscription, context.CancellationToken);

                    _logger.LogInformation(
                        "Changed old subscription {OldUserSubscriptionId} status from Current to Active for UserId: {UserId}",
                        currentSubscription.UserSubscriptionId, userSubscription.UserId);
                }

                // Change PendingUpgrade subscription to Current
                userSubscription.Status = "Current";
                await _userSubscriptionRepo.UpdateAsync(userSubscription, context.CancellationToken);

                _logger.LogInformation(
                    "Updated UserSubscription {UserSubscriptionId} status from PendingUpgrade to Current. UserId: {UserId}, PaymentMethod: {PaymentMethod}, Amount: {Amount}",
                    userSubscriptionId, paymentEvent.UserId, paymentEvent.PaymentMethod, paymentEvent.Amount);
            }
            else
            {
                _logger.LogInformation(
                    "UserSubscription {UserSubscriptionId} is not in Pending or PendingUpgrade status (Current: {Status}). Skipping update.",
                    userSubscriptionId, userSubscription.Status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user subscription for OrderId: {OrderId}", paymentEvent.OrderId);
            throw;
        }
    }
}
