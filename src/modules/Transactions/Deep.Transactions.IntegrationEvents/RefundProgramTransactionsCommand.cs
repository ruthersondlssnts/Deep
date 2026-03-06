using Deep.Common.Application.IntegrationEvents;

namespace Deep.Transactions.IntegrationEvents;

public sealed class RefundProgramTransactionsCommand(
    Guid id,
    DateTime occurredAtUtc,
    Guid programId,
    string reason) : IntegrationEvent(id, occurredAtUtc)
{
    public Guid ProgramId { get; } = programId;
    public string Reason { get; } = reason;
}
