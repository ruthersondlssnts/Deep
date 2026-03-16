using Deep.Common.Application.IntegrationEvents;

namespace Deep.Transactions.IntegrationEvents;

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
