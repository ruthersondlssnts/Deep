using Deep.Common.Application.IntegrationEvents;

namespace Deep.Transactions.IntegrationEvents;

public sealed class ReleaseStockIntegrationEvent(
    Guid id,
    DateTime occurredAtUtc,
    Guid programId,
    string productSku,
    int quantity
) : IntegrationEvent(id, occurredAtUtc)
{
    public Guid ProgramId { get; } = programId;
    public string ProductSku { get; } = productSku;
    public int Quantity { get; } = quantity;
}
