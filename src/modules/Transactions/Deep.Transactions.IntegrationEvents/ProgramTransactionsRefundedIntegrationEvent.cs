using Deep.Common.Application.IntegrationEvents;

namespace Deep.Transactions.IntegrationEvents;

public sealed class ProgramTransactionsRefundedIntegrationEvent(
    Guid id,
    DateTime occurredAtUtc,
    Guid programId,
    int totalTransactionsRefunded,
    decimal totalAmountRefunded) : IntegrationEvent(id, occurredAtUtc)
{
    public Guid ProgramId { get; } = programId;
    public int TotalTransactionsRefunded { get; } = totalTransactionsRefunded;
    public decimal TotalAmountRefunded { get; } = totalAmountRefunded;
}
