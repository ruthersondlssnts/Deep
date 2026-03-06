using Deep.Common.Application.IntegrationEvents;

namespace Deep.Transactions.IntegrationEvents;

public sealed class ReserveStockCommand(
    Guid id,
    DateTime occurredAtUtc,
    Guid transactionId,
    Guid programId,
    string productSku,
    int quantity) : IntegrationEvent(id, occurredAtUtc)
{
    public Guid TransactionId { get; } = transactionId;
    public Guid ProgramId { get; } = programId;
    public string ProductSku { get; } = productSku;
    public int Quantity { get; } = quantity;
}
