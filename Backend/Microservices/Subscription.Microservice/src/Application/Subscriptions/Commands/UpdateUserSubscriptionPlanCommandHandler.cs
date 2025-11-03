using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Commands;

public sealed class UpdateUserSubscriptionPlanCommandHandler
    : IRequestHandler<UpdateUserSubscriptionPlanCommand, Result<UserSubscriptionDto>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserSubscriptionRepository _userSubscriptionRepository;

    public UpdateUserSubscriptionPlanCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        IUserSubscriptionRepository userSubscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _userSubscriptionRepository = userSubscriptionRepository;
    }

    public async Task<Result<UserSubscriptionDto>> Handle(
        UpdateUserSubscriptionPlanCommand request,
        CancellationToken cancellationToken)
    {
        var currentSubscription = await _userSubscriptionRepository.GetActiveByUserIdAsync(
            request.UserId,
            cancellationToken);

        if (currentSubscription is null)
        {
            return Result.Failure<UserSubscriptionDto>(SubscriptionErrors.UserSubscriptionNotFound(request.UserId));
        }

        var currentPlan = currentSubscription.Subscription;
        if (currentPlan is null || !currentPlan.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<UserSubscriptionDto>(SubscriptionErrors.CurrentSubscriptionInactive(request.UserId));
        }

        if (currentSubscription.SubscriptionId == request.SubscriptionId)
        {
            return Result.Failure<UserSubscriptionDto>(SubscriptionErrors.SubscriptionAlreadyCurrent(request.SubscriptionId));
        }

        var newPlan = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);
        if (newPlan is null)
        {
            return Result.Failure<UserSubscriptionDto>(SubscriptionErrors.NotFound(request.SubscriptionId));
        }

        if (!newPlan.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<UserSubscriptionDto>(SubscriptionErrors.InactiveSubscription(request.SubscriptionId));
        }

        if (newPlan.OriginalPrice <= currentPlan.OriginalPrice)
        {
            return Result.Failure<UserSubscriptionDto>(SubscriptionErrors.UpgradeRequiresHigherPrice(request.SubscriptionId));
        }

        // Deactivate all existing active subscriptions
        await _userSubscriptionRepository.DeactivateUserSubscriptionsAsync(
            request.UserId,
            cancellationToken);

        // Determine expiration date based on subscription name
        int daysToAdd = 30; // Default: monthly subscription
        var subscriptionName = newPlan.Name.ToLower();

        if (subscriptionName.Contains("nam")) // "goi nam" = yearly
        {
            daysToAdd = 365;
        }
        else if (subscriptionName.Contains("thang")) // "goi thang" = monthly
        {
            daysToAdd = 30;
        }

        // Create new user subscription with Current status
        var newUserSubscription = new Domain.Entities.UserSubscription
        {
            UserSubscriptionId = Guid.NewGuid(),
            SubscriptionId = request.SubscriptionId,
            UserId = request.UserId,
            Status = "Current",
            SubscribedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(daysToAdd),
            Subscription = newPlan
        };

        await _userSubscriptionRepository.CreateAsync(newUserSubscription, cancellationToken);

        return Result.Success(UserSubscriptionDto.FromEntity(newUserSubscription));
    }
}

