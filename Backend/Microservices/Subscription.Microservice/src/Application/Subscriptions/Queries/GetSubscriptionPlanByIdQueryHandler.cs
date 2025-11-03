using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Queries;

public sealed class GetSubscriptionPlanByIdQueryHandler
    : IRequestHandler<GetSubscriptionPlanByIdQuery, Result<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _repository;

    public GetSubscriptionPlanByIdQueryHandler(ISubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<SubscriptionDto>> Handle(
        GetSubscriptionPlanByIdQuery request,
        CancellationToken cancellationToken)
    {
        var subscription = await _repository.GetByIdAsync(request.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            return Result.Failure<SubscriptionDto>(SubscriptionErrors.NotFound(request.SubscriptionId));
        }

        return Result.Success(SubscriptionDto.FromEntity(subscription));
    }
}
