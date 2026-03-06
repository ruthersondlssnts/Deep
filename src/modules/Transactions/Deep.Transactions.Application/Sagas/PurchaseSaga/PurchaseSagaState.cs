using MassTransit;

namespace Deep.Transactions.Application.Sagas.PurchaseSaga;

public sealed class PurchaseSagaState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public int Version { get; set; }

    public string CurrentState { get; set; } = string.Empty;

    public Guid TransactionId { get; set; }
    public Guid ProgramId { get; set; }
    public Guid CustomerId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? StockReservedAtUtc { get; set; }
    public DateTime? PaymentStartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? FailedAtUtc { get; set; }

    public string? FailureReason { get; set; }
    public string? PaymentReference { get; set; }

    public Guid? PaymentTimeoutToken { get; set; }
}
