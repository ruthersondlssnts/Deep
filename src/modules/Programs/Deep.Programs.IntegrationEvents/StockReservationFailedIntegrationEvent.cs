using Deep.Common.Application.IntegrationEvents;

namespace Deep.Programs.IntegrationEvents;

public sealed class StockReservationFailedIntegrationEvent(
    Guid id,
    DateTime occurredAtUtc,
    Guid transactionId,
    Guid programId,
    string productSku,
    int quantity,
    string reason) : IntegrationEvent(id, occurredAtUtc)
{
    public Guid TransactionId { get; } = transactionId;
    public Guid ProgramId { get; } = programId;
    public string ProductSku { get; } = productSku;
    public int Quantity { get; } = quantity;
    public string Reason { get; } = reason;
}
