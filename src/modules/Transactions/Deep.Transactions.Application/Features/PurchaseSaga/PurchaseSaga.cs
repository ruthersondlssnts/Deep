using Deep.Programs.IntegrationEvents;
using Deep.Transactions.IntegrationEvents;
using MassTransit;

namespace Deep.Transactions.Application.Features.PurchaseSaga;

public sealed class PurchaseSaga : MassTransitStateMachine<PurchaseSagaState>
{
    public State ReservingStock { get; private set; } = null!;
    public State ProcessingPayment { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    public Event<TransactionCreatedIntegrationEvent> TransactionCreated { get; private set; } =
        null!;
    public Event<StockReservedIntegrationEvent> StockReserved { get; private set; } = null!;
    public Event<StockReservationFailedIntegrationEvent> StockReservationFailed
    {
        get;
        private set;
    } = null!;
    public Event<PaymentCompletedIntegrationEvent> PaymentCompleted { get; private set; } = null!;
    public Event<PaymentFailedIntegrationEvent> PaymentFailed { get; private set; } = null!;

    public PurchaseSaga()
    {
        InstanceState(x => x.CurrentState);

        ConfigureEvents();

        Initially(WhenTransactionCreated());
        During(ReservingStock, WhenStockReserved(), WhenStockReservationFailed());
        During(ProcessingPayment, WhenPaymentCompleted(), WhenPaymentFailed());

        SetCompletedWhenFinalized();
    }

    private void ConfigureEvents()
    {
        Event(() => TransactionCreated, e => e.CorrelateById(ctx => ctx.Message.TransactionId));
        Event(() => StockReserved, e => e.CorrelateById(ctx => ctx.Message.TransactionId));
        Event(() => StockReservationFailed, e => e.CorrelateById(ctx => ctx.Message.TransactionId));
        Event(() => PaymentCompleted, e => e.CorrelateById(ctx => ctx.Message.TransactionId));
        Event(() => PaymentFailed, e => e.CorrelateById(ctx => ctx.Message.TransactionId));
    }

    private EventActivityBinder<
        PurchaseSagaState,
        TransactionCreatedIntegrationEvent
    > WhenTransactionCreated() =>
        When(TransactionCreated)
            .Then(ctx => ctx.Saga.CaptureTransactionData(ctx.Message))
            .TransitionTo(ReservingStock);

    private EventActivityBinder<
        PurchaseSagaState,
        StockReservedIntegrationEvent
    > WhenStockReserved() =>
        When(StockReserved)
            .Then(ctx => ctx.Saga.CaptureStockReserved(ctx.Message))
            .TransitionTo(ProcessingPayment)
            .Publish(ctx => new ProcessPaymentIntegrationEvent(
                Guid.CreateVersion7(),
                DateTime.UtcNow,
                ctx.Saga.TransactionId,
                ctx.Saga.CustomerId,
                ctx.Saga.TotalAmount
            ));

    private EventActivityBinder<
        PurchaseSagaState,
        StockReservationFailedIntegrationEvent
    > WhenStockReservationFailed() =>
        When(StockReservationFailed)
            .Then(ctx => ctx.Saga.FailureReason = ctx.Message.Reason)
            .Publish(ctx => new TransactionFailedIntegrationEvent(
                Guid.CreateVersion7(),
                DateTime.UtcNow,
                ctx.Saga.TransactionId,
                ctx.Saga.ProgramId,
                ctx.Saga.ProductSku,
                ctx.Saga.Quantity,
                ctx.Message.Reason
            ))
            .TransitionTo(Failed)
            .Finalize();

    private EventActivityBinder<
        PurchaseSagaState,
        PaymentCompletedIntegrationEvent
    > WhenPaymentCompleted() =>
        When(PaymentCompleted)
            .Then(ctx => ctx.Saga.PaymentReference = ctx.Message.PaymentReference)
            .Publish(ctx => new ConfirmStockIntegrationEvent(
                Guid.CreateVersion7(),
                DateTime.UtcNow,
                ctx.Saga.TransactionId,
                ctx.Saga.ProgramId,
                ctx.Saga.ProductSku,
                ctx.Saga.Quantity
            ))
            .Publish(ctx => new TransactionCompletedIntegrationEvent(
                Guid.CreateVersion7(),
                DateTime.UtcNow,
                ctx.Saga.TransactionId,
                ctx.Saga.ProgramId,
                ctx.Saga.ProductSku,
                ctx.Saga.Quantity,
                ctx.Saga.TotalAmount,
                ctx.Saga.PaymentReference!
            ))
            .TransitionTo(Completed)
            .Finalize();

    private EventActivityBinder<
        PurchaseSagaState,
        PaymentFailedIntegrationEvent
    > WhenPaymentFailed() =>
        When(PaymentFailed)
            .Then(ctx => ctx.Saga.FailureReason = ctx.Message.Reason)
            .Publish(ctx => new ReleaseStockIntegrationEvent(
                Guid.CreateVersion7(),
                DateTime.UtcNow,
                ctx.Saga.ProgramId,
                ctx.Saga.ProductSku,
                ctx.Saga.Quantity
            ))
            .Publish(ctx => new TransactionFailedIntegrationEvent(
                Guid.CreateVersion7(),
                DateTime.UtcNow,
                ctx.Saga.TransactionId,
                ctx.Saga.ProgramId,
                ctx.Saga.ProductSku,
                ctx.Saga.Quantity,
                ctx.Message.Reason
            ))
            .TransitionTo(Failed)
            .Finalize();
}

file static class PurchaseSagaStateExtensions
{
    public static void CaptureTransactionData(
        this PurchaseSagaState state,
        TransactionCreatedIntegrationEvent message
    )
    {
        state.TransactionId = message.TransactionId;
        state.ProgramId = message.ProgramId;
        state.CustomerId = message.CustomerId;
        state.ProductSku = message.ProductSku;
        state.Quantity = message.Quantity;
        state.TotalAmount = message.TotalAmount;
    }

    public static void CaptureStockReserved(
        this PurchaseSagaState state,
        StockReservedIntegrationEvent message
    )
    {
        state.UnitPrice = message.UnitPrice;
        state.TotalAmount = state.Quantity * message.UnitPrice;
    }
}
