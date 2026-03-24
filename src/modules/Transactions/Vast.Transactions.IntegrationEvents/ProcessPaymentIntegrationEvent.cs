using Vast.Common.Application.IntegrationEvents;

namespace Vast.Transactions.IntegrationEvents;

public sealed class ProcessPaymentIntegrationEvent(
    Guid id,
    DateTime occurredAtUtc,
    Guid transactionId,
    Guid customerId,
    decimal amount
) : IntegrationEvent(id, occurredAtUtc)
{
    public Guid TransactionId { get; } = transactionId;
    public Guid CustomerId { get; } = customerId;
    public decimal Amount { get; } = amount;
}
