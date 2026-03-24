using Vast.Common.Application.IntegrationEvents;

namespace Vast.Transactions.IntegrationEvents;

public sealed class PaymentFailedIntegrationEvent(
    Guid id,
    DateTime occurredAtUtc,
    Guid transactionId,
    string reason
) : IntegrationEvent(id, occurredAtUtc)
{
    public Guid TransactionId { get; } = transactionId;
    public string Reason { get; } = reason;
}
