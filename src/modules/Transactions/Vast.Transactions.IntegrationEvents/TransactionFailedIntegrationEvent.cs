using Vast.Common.Application.IntegrationEvents;

namespace Vast.Transactions.IntegrationEvents;

public sealed class TransactionFailedIntegrationEvent(
    Guid id,
    DateTime occurredAtUtc,
    Guid transactionId,
    Guid programId,
    string productSku,
    int quantity,
    string reason
) : IntegrationEvent(id, occurredAtUtc)
{
    public Guid TransactionId { get; } = transactionId;
    public Guid ProgramId { get; } = programId;
    public string ProductSku { get; } = productSku;
    public int Quantity { get; } = quantity;
    public string Reason { get; } = reason;
}
