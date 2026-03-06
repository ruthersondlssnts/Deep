using Deep.Programs.IntegrationEvents;
using Deep.Transactions.IntegrationEvents;
using MassTransit;

namespace Deep.Transactions.Application.Sagas.CancelProgramSaga;

public sealed class CancelProgramSaga : MassTransitStateMachine<CancelProgramSagaState>
{
    public State RefundingTransactions { get; private set; } = null!;
    public State RestoringStock { get; private set; } = null!;
    public State Completed { get; private set; } = null!;

    public Event<ProgramCancelledIntegrationEvent> ProgramCancelled { get; private set; } = null!;
    public Event<ProgramTransactionsRefundedIntegrationEvent> TransactionsRefunded { get; private set; } = null!;
    public Event<AllStockRestoredEvent> AllStockRestored { get; private set; } = null!;

    public Event CancellationCompleted { get; private set; } = null!;

    public CancelProgramSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => ProgramCancelled, e => e.CorrelateById(ctx => ctx.Message.ProgramId));
        Event(() => TransactionsRefunded, e => e.CorrelateById(ctx => ctx.Message.ProgramId));
        Event(() => AllStockRestored, e => e.CorrelateById(ctx => ctx.Message.ProgramId));

        CompositeEvent(
            () => CancellationCompleted,
            state => state.CancellationCompletedStatus,
            TransactionsRefunded,
            AllStockRestored);

        Initially(
            When(ProgramCancelled)
                .Then(ctx =>
                {
                    ctx.Saga.ProgramId = ctx.Message.ProgramId;
                    ctx.Saga.CancellationReason = ctx.Message.Reason;
                    ctx.Saga.CancellationStartedAtUtc = ctx.Message.OccurredAtUtc;
                })
                .Publish(ctx => new RefundProgramTransactionsCommand(
                    Guid.CreateVersion7(),
                    DateTime.UtcNow,
                    ctx.Saga.ProgramId,
                    ctx.Saga.CancellationReason))
                .TransitionTo(RefundingTransactions));

        During(RefundingTransactions,
            When(TransactionsRefunded)
                .Then(ctx =>
                {
                    ctx.Saga.TransactionsRefunded = ctx.Message.TotalTransactionsRefunded;
                    ctx.Saga.TotalAmountRefunded = ctx.Message.TotalAmountRefunded;
                    ctx.Saga.RefundsCompletedAtUtc = DateTime.UtcNow;
                })
                .TransitionTo(RestoringStock));

        During(RestoringStock,
            When(AllStockRestored)
                .Then(ctx =>
                {
                    ctx.Saga.StockRestoredAtUtc = DateTime.UtcNow;
                }));

        DuringAny(
            When(CancellationCompleted)
                .Then(ctx =>
                {
                    ctx.Saga.CompletedAtUtc = DateTime.UtcNow;
                })
                .Publish(ctx => new ProgramCancellationCompletedIntegrationEvent(
                    Guid.CreateVersion7(),
                    DateTime.UtcNow,
                    ctx.Saga.ProgramId,
                    ctx.Saga.TransactionsRefunded,
                    ctx.Saga.TotalAmountRefunded))
                .TransitionTo(Completed)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}

public sealed record AllStockRestoredEvent(Guid ProgramId);
