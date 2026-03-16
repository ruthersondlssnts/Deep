using Deep.Common.Application.IntegrationEvents;

namespace Deep.Transactions.IntegrationEvents;

public sealed class TransactionCreatedIntegrationEvent(
    Guid id,
    DateTime occurredAtUtc,
    Guid transactionId,
    Guid programId,
    Guid customerId,
    string productSku,
    int quantity,
    decimal totalAmount
) : IntegrationEvent(id, occurredAtUtc)
{
    public Guid TransactionId { get; } = transactionId;
    public Guid ProgramId { get; } = programId;
    public Guid CustomerId { get; } = customerId;
    public string ProductSku { get; } = productSku;
    public int Quantity { get; } = quantity;
    public decimal TotalAmount { get; } = totalAmount;
}
