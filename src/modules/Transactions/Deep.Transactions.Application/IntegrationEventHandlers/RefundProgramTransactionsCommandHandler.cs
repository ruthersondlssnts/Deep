using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Domain;
using Deep.Transactions.Application.Data;
using Deep.Transactions.Domain.Transaction;
using Deep.Transactions.IntegrationEvents;
using Microsoft.EntityFrameworkCore;

namespace Deep.Transactions.Application.IntegrationEventHandlers;

internal sealed class RefundProgramTransactionsCommandHandler(
    TransactionsDbContext context,
    IEventBus eventBus
) : IntegrationEventHandler<RefundProgramTransactionsCommand>
{
    public override async Task Handle(
        RefundProgramTransactionsCommand command,
        CancellationToken cancellationToken = default)
    {
        List<Transaction> completedTransactions = await context.Transactions
            .Where(t => t.ProgramId == command.ProgramId &&
                       t.Status == TransactionStatus.Completed)
            .ToListAsync(cancellationToken);

        int totalRefunded = 0;
        decimal totalAmount = 0;

        foreach (Transaction transaction in completedTransactions)
        {
            string refundReference = $"REF-{Guid.CreateVersion7():N}"[..20];

            Result refundResult = transaction.Refund(refundReference);

            if (refundResult.IsSuccess)
            {
                totalRefunded++;
                totalAmount += transaction.TotalAmount;

                await eventBus.PublishAsync(
                    new RestoreStockCommand(
                        Guid.CreateVersion7(),
                        DateTime.UtcNow,
                        transaction.Id,
                        transaction.ProgramId,
                        transaction.ProductSku,
                        transaction.Quantity),
                    cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        await eventBus.PublishAsync(
            new ProgramTransactionsRefundedIntegrationEvent(
                Guid.CreateVersion7(),
                DateTime.UtcNow,
                command.ProgramId,
                totalRefunded,
                totalAmount),
            cancellationToken);
    }
}
