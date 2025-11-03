using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Commands;

public sealed class ChooseSubscriptionPlanCommandHandler
    : IRequestHandler<ChooseSubscriptionPlanCommand, Result<UserSubscriptionDto>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserSubscriptionRepository _userSubscriptionRepository;

    public ChooseSubscriptionPlanCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        IUserSubscriptionRepository userSubscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _userSubscriptionRepository = userSubscriptionRepository;
    }

    public async Task<Result<UserSubscriptionDto>> Handle(
        ChooseSubscriptionPlanCommand request,
        CancellationToken cancellationToken)
    {
        // Verify subscription plan exists
        var subscription = await _subscriptionRepository.GetByIdAsync(
            request.SubscriptionId,
            cancellationToken);

        if (subscription is null)
        {
            return Result.Failure<UserSubscriptionDto>(
                SubscriptionErrors.NotFound(request.SubscriptionId));
        }

        // Check if subscription plan is active
        if (!subscription.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<UserSubscriptionDto>(
                SubscriptionErrors.InactiveSubscription(request.SubscriptionId));
        }

        // Deactivate all existing active subscriptions for this user
        await _userSubscriptionRepository.DeactivateUserSubscriptionsAsync(
            request.UserId,
            cancellationToken);

        // Determine expiration date based on subscription name
        int daysToAdd = 30; // Default: monthly subscription
        var subscriptionName = subscription.Name.ToLower();

        if (subscriptionName.Contains("nam")) // "goi nam" = yearly
        {
            daysToAdd = 365;
        }
        else if (subscriptionName.Contains("thang")) // "goi thang" = monthly
        {
            daysToAdd = 30;
        }

        // Create new user subscription with Current status
        var userSubscription = new UserSubscription
        {
            UserSubscriptionId = Guid.NewGuid(),
            SubscriptionId = request.SubscriptionId,
            UserId = request.UserId,
            Status = "Current",
            SubscribedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(daysToAdd),
            Subscription = subscription
        };

        await _userSubscriptionRepository.CreateAsync(userSubscription, cancellationToken);

        return Result.Success(UserSubscriptionDto.FromEntity(userSubscription));
    }
}
