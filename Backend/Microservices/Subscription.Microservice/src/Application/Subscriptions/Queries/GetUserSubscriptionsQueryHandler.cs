using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Queries;

public sealed class GetUserSubscriptionsQueryHandler
    : IRequestHandler<GetUserSubscriptionsQuery, Result<IReadOnlyList<UserSubscriptionDto>>>
{
    private readonly IUserSubscriptionRepository _repository;

    public GetUserSubscriptionsQueryHandler(IUserSubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<UserSubscriptionDto>>> Handle(
        GetUserSubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        var userSubscriptions = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        var dtos = userSubscriptions
            .Select(UserSubscriptionDto.FromEntity)
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<UserSubscriptionDto>>(dtos);
    }
}

