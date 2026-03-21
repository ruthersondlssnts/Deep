using Deep.Common.Application.IntegrationEvents;
using Deep.Transactions.IntegrationEvents;

namespace Deep.Transactions.Application.IntegrationEventHandlers;

internal sealed class ProcessPaymentIntegrationHandler(IEventBus eventBus)
    : IntegrationEventHandler<ProcessPaymentIntegrationEvent>
{
    public override async Task Handle(
        ProcessPaymentIntegrationEvent command,
        CancellationToken cancellationToken = default
    )
    {
        await Task.Delay(3000, cancellationToken);

#pragma warning disable CA5394 // Do not use insecure randomness
        bool paymentSucceeded = Random.Shared.NextDouble() < 0.95;
#pragma warning restore CA5394 // Do not use insecure randomness

        if (paymentSucceeded)
        {
            string paymentReference = $"PAY-{Guid.CreateVersion7():N}"[..20];

            await eventBus.PublishAsync(
                new PaymentCompletedIntegrationEvent(
                    Guid.CreateVersion7(),
                    DateTime.UtcNow,
                    command.TransactionId,
                    paymentReference
                ),
                cancellationToken
            );
        }
        else
        {
            await eventBus.PublishAsync(
                new PaymentFailedIntegrationEvent(
                    Guid.CreateVersion7(),
                    DateTime.UtcNow,
                    command.TransactionId,
                    "Payment declined by provider"
                ),
                cancellationToken
            );
        }
    }
}
