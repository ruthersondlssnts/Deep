using Deep.Common.Domain;

namespace Deep.Transactions.Domain.Transaction;

public sealed class TransactionStockReservedDomainEvent(
    Guid transactionId,
    Guid programId,
    string productSku,
    int quantity) : DomainEvent
{
    public Guid TransactionId { get; } = transactionId;
    public Guid ProgramId { get; } = programId;
    public string ProductSku { get; } = productSku;
    public int Quantity { get; } = quantity;
}
