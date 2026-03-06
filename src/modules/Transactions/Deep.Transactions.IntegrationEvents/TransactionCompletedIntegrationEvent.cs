using Deep.Common.Application.IntegrationEvents;

namespace Deep.Transactions.IntegrationEvents;

public sealed class TransactionCompletedIntegrationEvent(
    Guid id,
    DateTime occurredAtUtc,
    Guid transactionId,
    Guid programId,
    string productSku,
    int quantity,
    decimal totalAmount,
    string paymentReference) : IntegrationEvent(id, occurredAtUtc)
{
    public Guid TransactionId { get; } = transactionId;
    public Guid ProgramId { get; } = programId;
    public string ProductSku { get; } = productSku;
    public int Quantity { get; } = quantity;
    public decimal TotalAmount { get; } = totalAmount;
    public string PaymentReference { get; } = paymentReference;
}
