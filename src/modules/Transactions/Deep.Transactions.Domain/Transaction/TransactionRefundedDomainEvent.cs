using Deep.Common.Domain;

namespace Deep.Transactions.Domain.Transaction;

public sealed class TransactionRefundedDomainEvent(
    Guid transactionId,
    Guid programId,
    string productSku,
    int quantity,
    decimal totalAmount,
    string refundReference) : DomainEvent
{
    public Guid TransactionId { get; } = transactionId;
    public Guid ProgramId { get; } = programId;
    public string ProductSku { get; } = productSku;
    public int Quantity { get; } = quantity;
    public decimal TotalAmount { get; } = totalAmount;
    public string RefundReference { get; } = refundReference;
}
