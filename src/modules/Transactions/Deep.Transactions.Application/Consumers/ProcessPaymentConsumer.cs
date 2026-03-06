using Deep.Transactions.Application.Sagas.PurchaseSaga;
using MassTransit;

namespace Deep.Transactions.Application.Consumers;

public sealed class ProcessPaymentConsumer : IConsumer<ProcessPaymentCommand>
{
    public async Task Consume(ConsumeContext<ProcessPaymentCommand> context)
    {
        ProcessPaymentCommand command = context.Message;

        await Task.Delay(Random.Shared.Next(100, 500), context.CancellationToken);

        bool paymentSucceeded = Random.Shared.NextDouble() < 0.95;

        if (paymentSucceeded)
        {
            string paymentReference = $"PAY-{Guid.CreateVersion7():N}"[..20];

            await context.Publish(
                new PaymentCompletedEvent(command.TransactionId, paymentReference),
                context.CancellationToken);
        }
        else
        {
            await context.Publish(
                new PaymentFailedEvent(command.TransactionId, "Payment declined by provider"),
                context.CancellationToken);
        }
    }
}
