using Deep.Programs.IntegrationEvents;
using Deep.Transactions.IntegrationEvents;
using MassTransit;

namespace Deep.Transactions.Application.Sagas.PurchaseSaga;

public sealed class PurchaseSaga : MassTransitStateMachine<PurchaseSagaState>
{
    public State ReservingStock { get; private set; } = null!;
    public State ProcessingPayment { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    public Event<TransactionCreatedIntegrationEvent> TransactionCreated { get; private set; } = null!;
    public Event<StockReservedIntegrationEvent> StockReserved { get; private set; } = null!;
    public Event<StockReservationFailedIntegrationEvent> StockReservationFailed { get; private set; } = null!;
    public Event<PaymentCompletedEvent> PaymentCompleted { get; private set; } = null!;
    public Event<PaymentFailedEvent> PaymentFailed { get; private set; } = null!;

    public Schedule<PurchaseSagaState, PaymentTimeoutExpired> PaymentTimeout { get; private set; } = null!;

    public PurchaseSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => TransactionCreated, e => e.CorrelateById(ctx => ctx.Message.TransactionId));
        Event(() => StockReserved, e => e.CorrelateById(ctx => ctx.Message.TransactionId));
        Event(() => StockReservationFailed, e => e.CorrelateById(ctx => ctx.Message.TransactionId));
        Event(() => PaymentCompleted, e => e.CorrelateById(ctx => ctx.Message.TransactionId));
        Event(() => PaymentFailed, e => e.CorrelateById(ctx => ctx.Message.TransactionId));

        Schedule(() => PaymentTimeout, instance => instance.PaymentTimeoutToken, s =>
        {
            s.Delay = TimeSpan.FromMinutes(5);
            s.Received = x => x.CorrelateById(ctx => ctx.Message.TransactionId);
        });

        Initially(
            When(TransactionCreated)
                .Then(ctx =>
                {
                    ctx.Saga.TransactionId = ctx.Message.TransactionId;
                    ctx.Saga.ProgramId = ctx.Message.ProgramId;
                    ctx.Saga.CustomerId = ctx.Message.CustomerId;
                    ctx.Saga.ProductSku = ctx.Message.ProductSku;
                    ctx.Saga.Quantity = ctx.Message.Quantity;
                    ctx.Saga.TotalAmount = ctx.Message.TotalAmount;
                    ctx.Saga.CreatedAtUtc = ctx.Message.OccurredAtUtc;
                })
                .Publish(ctx => new ReserveStockCommand(
                    Guid.CreateVersion7(),
                    DateTime.UtcNow,
                    ctx.Saga.TransactionId,
                    ctx.Saga.ProgramId,
                    ctx.Saga.ProductSku,
                    ctx.Saga.Quantity))
                .TransitionTo(ReservingStock));

        During(ReservingStock,
            When(StockReserved)
                .Then(ctx =>
                {
                    ctx.Saga.StockReservedAtUtc = ctx.Message.OccurredAtUtc;
                    ctx.Saga.UnitPrice = ctx.Message.UnitPrice;
                    ctx.Saga.TotalAmount = ctx.Saga.Quantity * ctx.Message.UnitPrice;
                    ctx.Saga.PaymentStartedAtUtc = DateTime.UtcNow;
                })
                .Publish(ctx => new ProcessPaymentCommand(
                    Guid.CreateVersion7(),
                    DateTime.UtcNow,
                    ctx.Saga.TransactionId,
                    ctx.Saga.CustomerId,
                    ctx.Saga.TotalAmount))
                .Schedule(PaymentTimeout, ctx => new PaymentTimeoutExpired(ctx.Saga.TransactionId))
                .TransitionTo(ProcessingPayment),

            When(StockReservationFailed)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.FailedAtUtc = DateTime.UtcNow;
                })
                .Publish(ctx => new TransactionFailedIntegrationEvent(
                    Guid.CreateVersion7(),
                    DateTime.UtcNow,
                    ctx.Saga.TransactionId,
                    ctx.Saga.ProgramId,
                    ctx.Saga.ProductSku,
                    ctx.Saga.Quantity,
                    ctx.Message.Reason))
                .TransitionTo(Failed)
                .Finalize());

        During(ProcessingPayment,
            When(PaymentCompleted)
                .Unschedule(PaymentTimeout)
                .Then(ctx =>
                {
                    ctx.Saga.PaymentReference = ctx.Message.PaymentReference;
                    ctx.Saga.CompletedAtUtc = DateTime.UtcNow;
                })
                .Publish(ctx => new ConfirmStockCommand(
                    Guid.CreateVersion7(),
                    DateTime.UtcNow,
                    ctx.Saga.TransactionId,
                    ctx.Saga.ProgramId,
                    ctx.Saga.ProductSku,
                    ctx.Saga.Quantity))
                .Publish(ctx => new TransactionCompletedIntegrationEvent(
                    Guid.CreateVersion7(),
                    DateTime.UtcNow,
                    ctx.Saga.TransactionId,
                    ctx.Saga.ProgramId,
                    ctx.Saga.ProductSku,
                    ctx.Saga.Quantity,
                    ctx.Saga.TotalAmount,
                    ctx.Message.PaymentReference))
                .TransitionTo(Completed)
                .Finalize(),

            When(PaymentFailed)
                .Unschedule(PaymentTimeout)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.FailedAtUtc = DateTime.UtcNow;
                })
                .Publish(ctx => new ReleaseStockCommand(
                    Guid.CreateVersion7(),
                    DateTime.UtcNow,
                    ctx.Saga.TransactionId,
                    ctx.Saga.ProgramId,
                    ctx.Saga.ProductSku,
                    ctx.Saga.Quantity))
                .Publish(ctx => new TransactionFailedIntegrationEvent(
                    Guid.CreateVersion7(),
                    DateTime.UtcNow,
                    ctx.Saga.TransactionId,
                    ctx.Saga.ProgramId,
                    ctx.Saga.ProductSku,
                    ctx.Saga.Quantity,
                    ctx.Message.Reason))
                .TransitionTo(Failed)
                .Finalize(),

            When(PaymentTimeout.Received)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = "Payment processing timed out";
                    ctx.Saga.FailedAtUtc = DateTime.UtcNow;
                })
                .Publish(ctx => new ReleaseStockCommand(
                    Guid.CreateVersion7(),
                    DateTime.UtcNow,
                    ctx.Saga.TransactionId,
                    ctx.Saga.ProgramId,
                    ctx.Saga.ProductSku,
                    ctx.Saga.Quantity))
                .Publish(ctx => new TransactionFailedIntegrationEvent(
                    Guid.CreateVersion7(),
                    DateTime.UtcNow,
                    ctx.Saga.TransactionId,
                    ctx.Saga.ProgramId,
                    ctx.Saga.ProductSku,
                    ctx.Saga.Quantity,
                    "Payment processing timed out"))
                .TransitionTo(Failed)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}

public sealed record ProcessPaymentCommand(
    Guid Id,
    DateTime OccurredAtUtc,
    Guid TransactionId,
    Guid CustomerId,
    decimal Amount);

public sealed record PaymentCompletedEvent(Guid TransactionId, string PaymentReference);

public sealed record PaymentFailedEvent(Guid TransactionId, string Reason);

public sealed record PaymentTimeoutExpired(Guid TransactionId);
