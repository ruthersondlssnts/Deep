using Deep.Common.Application.IntegrationEvents;
using Deep.Transactions.IntegrationEvents;

namespace Deep.Transactions.Application.IntegrationEventHandlers;

internal sealed class ProcessPaymentCommandHandler(
    IEventBus eventBus
) : IntegrationEventHandler<ProcessPaymentCommand>
{
    public override async Task Handle(
        ProcessPaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(Random.Shared.Next(100, 500), cancellationToken);

        bool paymentSucceeded = Random.Shared.NextDouble() < 0.95;

        if (paymentSucceeded)
        {
            string paymentReference = $"PAY-{Guid.CreateVersion7():N}"[..20];

            await eventBus.PublishAsync(
                new PaymentCompletedIntegrationEvent(
                    Guid.CreateVersion7(),
                    DateTime.UtcNow,
                    command.TransactionId,
                    paymentReference),
                cancellationToken);
        }
        else
        {
            await eventBus.PublishAsync(
                new PaymentFailedIntegrationEvent(
                    Guid.CreateVersion7(),
                    DateTime.UtcNow,
                    command.TransactionId,
                    "Payment declined by provider"),
                cancellationToken);
        }
    }
}
