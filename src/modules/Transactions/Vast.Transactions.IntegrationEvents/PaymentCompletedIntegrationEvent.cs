using Vast.Common.Application.IntegrationEvents;

namespace Vast.Transactions.IntegrationEvents;

public sealed class PaymentCompletedIntegrationEvent(
    Guid id,
    DateTime occurredAtUtc,
    Guid transactionId,
    string paymentReference
) : IntegrationEvent(id, occurredAtUtc)
{
    public Guid TransactionId { get; } = transactionId;
    public string PaymentReference { get; } = paymentReference;
}
