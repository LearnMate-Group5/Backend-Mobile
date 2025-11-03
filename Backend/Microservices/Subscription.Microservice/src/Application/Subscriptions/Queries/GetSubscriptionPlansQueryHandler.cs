using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Queries;

public sealed class GetSubscriptionPlansQueryHandler
    : IRequestHandler<GetSubscriptionPlansQuery, Result<IReadOnlyList<SubscriptionDto>>>
{
    private readonly ISubscriptionRepository _repository;

    public GetSubscriptionPlansQueryHandler(ISubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<SubscriptionDto>>> Handle(
        GetSubscriptionPlansQuery request,
        CancellationToken cancellationToken)
    {
        var subscriptions = await _repository.GetAllAsync(cancellationToken);

        var dtos = subscriptions
            .Select(SubscriptionDto.FromEntity)
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<SubscriptionDto>>(dtos);
    }
}
