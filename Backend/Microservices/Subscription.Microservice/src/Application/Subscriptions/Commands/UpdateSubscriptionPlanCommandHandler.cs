using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Commands;

public sealed class UpdateSubscriptionPlanCommandHandler
    : IRequestHandler<UpdateSubscriptionPlanCommand, Result<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _repository;

    public UpdateSubscriptionPlanCommandHandler(ISubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<SubscriptionDto>> Handle(
        UpdateSubscriptionPlanCommand request,
        CancellationToken cancellationToken)
    {
        var subscription =
            await _repository.GetByIdAsync(request.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            return Result.Failure<SubscriptionDto>(SubscriptionErrors.NotFound(request.SubscriptionId));
        }

        var updatedName = request.Name?.Trim() ?? subscription.Name;
        var updatedType = request.Type?.Trim() ?? subscription.Type;

        // Check if the combination of name and type already exists (excluding current subscription)
        if (request.Name is not null || request.Type is not null)
        {
            var nameAndTypeExists = await _repository.NameAndTypeExistsAsync(
                updatedName,
                updatedType,
                request.SubscriptionId,
                cancellationToken);

            if (nameAndTypeExists)
            {
                return Result.Failure<SubscriptionDto>(SubscriptionErrors.DuplicateNameAndType(updatedName, updatedType));
            }
        }

        if (request.Name is not null)
        {
            subscription.Name = updatedName;
        }

        if (request.Type is not null)
        {
            subscription.Type = updatedType;
        }

        if (request.Status is not null)
        {
            subscription.Status = request.Status.Trim();
        }

        if (request.OriginalPrice.HasValue)
        {
            subscription.OriginalPrice = request.OriginalPrice.Value;
        }

        if (request.Discount.HasValue)
        {
            subscription.Discount = request.Discount.Value;
        }

        subscription.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(subscription, cancellationToken);

        return Result.Success(SubscriptionDto.FromEntity(subscription));
    }
}
