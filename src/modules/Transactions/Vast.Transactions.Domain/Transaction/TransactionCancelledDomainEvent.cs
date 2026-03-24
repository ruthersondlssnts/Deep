using Vast.Common.Domain;

namespace Vast.Transactions.Domain.Transaction;

public sealed class TransactionCancelledDomainEvent(
    Guid transactionId,
    Guid programId,
    string productSku,
    int quantity,
    string reason
) : DomainEvent
{
    public Guid TransactionId { get; } = transactionId;
    public Guid ProgramId { get; } = programId;
    public string ProductSku { get; } = productSku;
    public int Quantity { get; } = quantity;
    public string Reason { get; } = reason;
}
