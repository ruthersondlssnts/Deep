using Deep.Common.Application.IntegrationEvents;

namespace Deep.Programs.IntegrationEvents;

public sealed class ProgramCancellationCompletedIntegrationEvent(
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
