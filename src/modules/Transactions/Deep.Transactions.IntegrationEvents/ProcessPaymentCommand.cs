using Deep.Common.Application.IntegrationEvents;

namespace Deep.Transactions.IntegrationEvents;

public sealed class ProcessPaymentCommand(
    Guid id,
    DateTime occurredAtUtc,
    Guid transactionId,
    Guid customerId,
    decimal amount) : IntegrationEvent(id, occurredAtUtc)
{
    public Guid TransactionId { get; } = transactionId;
    public Guid CustomerId { get; } = customerId;
    public decimal Amount { get; } = amount;
}
