using Vast.Common.Application.DomainEvents;
using Vast.Common.Application.Exceptions;
using Vast.Common.Application.IntegrationEvents;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;
using Vast.Transactions.Domain.Transaction;
using Vast.Transactions.IntegrationEvents;

namespace Vast.Transactions.Application.Features.Transactions;

internal sealed class TransactionCreatedDomainEventHandler(
    IRequestBus requestBus,
    IEventBus eventBus
) : DomainEventHandler<TransactionCreatedDomainEvent>
{
    public override async Task Handle(
        TransactionCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<GetTransaction.Response> result = await requestBus.Send<GetTransaction.Response>(
            new GetTransaction.Query(domainEvent.TransactionId),
            cancellationToken
        );
        if (result.IsFailure)
        {
            throw new VastException(nameof(GetTransaction), result.Error);
        }

        GetTransaction.Response transaction = result.Value;

        await eventBus.PublishAsync(
            new TransactionCreatedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredAtUtc,
                transaction.Id,
                transaction.ProgramId,
                transaction.CustomerId,
                transaction.ProductSku,
                transaction.Quantity,
                transaction.TotalAmount
            ),
            cancellationToken
        );
    }
}
