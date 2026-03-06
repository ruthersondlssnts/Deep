using MassTransit;

namespace Deep.Transactions.Application.Sagas.CancelProgramSaga;

public sealed class CancelProgramSagaState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public int Version { get; set; }

    public string CurrentState { get; set; } = string.Empty;

    public Guid ProgramId { get; set; }
    public string CancellationReason { get; set; } = string.Empty;

    public DateTime CancellationStartedAtUtc { get; set; }
    public DateTime? RefundsCompletedAtUtc { get; set; }
    public DateTime? StockRestoredAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public int TotalTransactionsToRefund { get; set; }
    public int TransactionsRefunded { get; set; }
    public decimal TotalAmountRefunded { get; set; }

    public int CancellationCompletedStatus { get; set; }
}
