using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Queries;

public sealed class GetActiveUserSubscriptionQueryHandler
    : IRequestHandler<GetActiveUserSubscriptionQuery, Result<UserSubscriptionDto>>
{
    private readonly IUserSubscriptionRepository _repository;

    public GetActiveUserSubscriptionQueryHandler(IUserSubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<UserSubscriptionDto>> Handle(
        GetActiveUserSubscriptionQuery request,
        CancellationToken cancellationToken)
    {
        var userSubscription = await _repository.GetActiveByUserIdAsync(request.UserId, cancellationToken);

        if (userSubscription is null)
        {
            return Result.Failure<UserSubscriptionDto>(SubscriptionErrors.UserSubscriptionNotFound(request.UserId));
        }

        return Result.Success(UserSubscriptionDto.FromEntity(userSubscription));
    }
}

