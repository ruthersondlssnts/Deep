using Deep.Common.Domain;

namespace Deep.Transactions.Domain.Transaction;

public sealed class TransactionFailedDomainEvent(
    Guid transactionId,
    Guid programId,
    string productSku,
    int quantity,
    string reason) : DomainEvent
{
    public Guid TransactionId { get; } = transactionId;
    public Guid ProgramId { get; } = programId;
    public string ProductSku { get; } = productSku;
    public int Quantity { get; } = quantity;
    public string Reason { get; } = reason;
}
