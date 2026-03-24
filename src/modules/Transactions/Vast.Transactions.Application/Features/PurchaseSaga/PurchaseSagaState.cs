using MassTransit;

namespace Vast.Transactions.Application.Features.PurchaseSaga;

public sealed class PurchaseSagaState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public int Version { get; set; }
    public string CurrentState { get; set; } = string.Empty;

    // Transaction context
    public Guid TransactionId { get; set; }
    public Guid ProgramId { get; set; }
    public Guid CustomerId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }

    // Payment
    public string? PaymentReference { get; set; }
    public string? FailureReason { get; set; }
}
