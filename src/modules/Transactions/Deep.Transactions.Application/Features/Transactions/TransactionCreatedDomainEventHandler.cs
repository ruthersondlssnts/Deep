using Deep.Common.Application.DomainEvents;
using Deep.Common.Application.IntegrationEvents;
using Deep.Transactions.Application.Data;
using Deep.Transactions.Domain.Transaction;
using Deep.Transactions.IntegrationEvents;
using Microsoft.EntityFrameworkCore;

namespace Deep.Transactions.Application.Features.Transactions;

internal sealed class TransactionCreatedDomainEventHandler(
    TransactionsDbContext context,
    IEventBus eventBus
) : DomainEventHandler<TransactionCreatedDomainEvent>
{
    public override async Task Handle(
        TransactionCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Transaction? transaction = await context.Transactions
            .FirstOrDefaultAsync(t => t.Id == domainEvent.TransactionId, cancellationToken);

        if (transaction is null)
        {
            return;
        }

        await eventBus.PublishAsync(
            new TransactionCreatedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredAtUtc,
                transaction.Id,
                transaction.ProgramId,
                transaction.CustomerId,
                transaction.ProductSku,
                transaction.Quantity,
                transaction.TotalAmount),
            cancellationToken);
    }
}
