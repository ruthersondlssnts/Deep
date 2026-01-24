using Deep.Common.Domain;
using Deep.Common.EventBus;

namespace Deep.Transactions.IntegrationEvents
{
    public sealed class TransactionCreatedIntegrationEvent(
        Guid id,
        DateTime occurredAtUtc,
        int totalTransactions,
        int totalCustomers,
        Guid programId)
    : IntegrationEvent(id, occurredAtUtc)
    {
        public int TotalTransactions { get; } = totalTransactions;
        public int TotalCustomers { get; } = totalCustomers;
        public Guid ProgramId { get; } = programId;
    }
}
