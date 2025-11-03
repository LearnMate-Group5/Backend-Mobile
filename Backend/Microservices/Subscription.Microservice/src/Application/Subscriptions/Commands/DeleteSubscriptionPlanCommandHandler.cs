using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Commands;

public sealed class DeleteSubscriptionPlanCommandHandler
    : IRequestHandler<DeleteSubscriptionPlanCommand, Result>
{
    private readonly ISubscriptionRepository _repository;

    public DeleteSubscriptionPlanCommandHandler(ISubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _repository.GetByIdAsync(request.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            return Result.Failure(SubscriptionErrors.NotFound(request.SubscriptionId));
        }

        await _repository.DeleteAsync(subscription, cancellationToken);

        return Result.Success();
    }
}
