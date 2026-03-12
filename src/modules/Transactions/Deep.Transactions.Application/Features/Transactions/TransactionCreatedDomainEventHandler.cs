using Deep.Common.Application.DomainEvents;
using Deep.Common.Application.Exceptions;
using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Transactions.Domain.Transaction;
using Deep.Transactions.IntegrationEvents;

namespace Deep.Transactions.Application.Features.Transactions;

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
            throw new DeepException(nameof(GetTransaction), result.Error);
        }

        var transaction = result.Value;

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
