using Vast.Common.Domain;

namespace Vast.Transactions.Domain.Transaction;

public sealed class TransactionCreatedDomainEvent(
    Guid transactionId,
    Guid programId,
    string productSku,
    int quantity,
    decimal totalAmount
) : DomainEvent
{
    public Guid TransactionId { get; } = transactionId;
    public Guid ProgramId { get; } = programId;
    public string ProductSku { get; } = productSku;
    public int Quantity { get; } = quantity;
    public decimal TotalAmount { get; } = totalAmount;
}
