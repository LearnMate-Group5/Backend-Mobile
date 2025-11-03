using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.Subscriptions.Commands;

public sealed class CreateSubscriptionPlanCommandHandler
    : IRequestHandler<CreateSubscriptionPlanCommand, Result<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _repository;

    public CreateSubscriptionPlanCommandHandler(ISubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<SubscriptionDto>> Handle(
        CreateSubscriptionPlanCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim();
        var normalizedType = request.Type.Trim();

        var nameAndTypeExists = await _repository.NameAndTypeExistsAsync(normalizedName, normalizedType, null, cancellationToken);
        if (nameAndTypeExists)
        {
            return Result.Failure<SubscriptionDto>(SubscriptionErrors.DuplicateNameAndType(normalizedName, normalizedType));
        }

        var subscription = new Subscription
        {
            SubscriptionId = Guid.NewGuid(),
            Name = normalizedName,
            Type = normalizedType,
            Status = request.Status.Trim(),
            OriginalPrice = request.OriginalPrice,
            Discount = request.Discount
        };

        await _repository.CreateAsync(subscription, cancellationToken);

        return Result.Success(SubscriptionDto.FromEntity(subscription));
    }
}
