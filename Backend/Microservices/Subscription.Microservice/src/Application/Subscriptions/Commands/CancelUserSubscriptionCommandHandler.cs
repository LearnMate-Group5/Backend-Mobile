using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Commands;

public sealed class CancelUserSubscriptionCommandHandler
    : IRequestHandler<CancelUserSubscriptionCommand, Result<UserSubscriptionDto>>
{
    private readonly IUserSubscriptionRepository _userSubscriptionRepository;

    public CancelUserSubscriptionCommandHandler(IUserSubscriptionRepository userSubscriptionRepository)
    {
        _userSubscriptionRepository = userSubscriptionRepository;
    }

    public async Task<Result<UserSubscriptionDto>> Handle(
        CancelUserSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        // Get the active subscription
        var activeSubscription = await _userSubscriptionRepository.GetActiveByUserIdAsync(
            request.UserId,
            cancellationToken);

        if (activeSubscription is null)
        {
            return Result.Failure<UserSubscriptionDto>(
                SubscriptionErrors.UserSubscriptionNotFound(request.UserId));
        }

        // Check if already cancelled
        if (activeSubscription.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<UserSubscriptionDto>(
                SubscriptionErrors.SubscriptionAlreadyCancelled(request.UserId));
        }

        // Set status to Cancelled but keep it usable until ExpiredAt
        activeSubscription.Status = "Cancelled";

        // If no expiration date is set, set it to 30 days from now (default subscription period)
        if (!activeSubscription.ExpiredAt.HasValue)
        {
            activeSubscription.ExpiredAt = DateTime.UtcNow.AddDays(30);
        }

        await _userSubscriptionRepository.UpdateAsync(activeSubscription, cancellationToken);

        return Result.Success(UserSubscriptionDto.FromEntity(activeSubscription));
    }
}
