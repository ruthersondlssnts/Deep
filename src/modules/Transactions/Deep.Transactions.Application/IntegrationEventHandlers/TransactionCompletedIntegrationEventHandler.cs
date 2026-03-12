using Deep.Common.Application.Exceptions;
using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Transactions.Application.Features.Transactions;
using Deep.Transactions.Domain.Transaction;
using Deep.Transactions.IntegrationEvents;

namespace Deep.Transactions.Application.IntegrationEventHandlers;

internal sealed class TransactionCompletedIntegrationEventHandler(IRequestBus requestBus)
    : IntegrationEventHandler<TransactionCompletedIntegrationEvent>
{
    public override async Task Handle(
        TransactionCompletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<UpdateTransactionStatus.Response> result =
            await requestBus.Send<UpdateTransactionStatus.Response>(
                new UpdateTransactionStatus.Command(
                    integrationEvent.TransactionId,
                    TransactionStatus.Completed,
                    integrationEvent.PaymentReference
                ),
                cancellationToken
            );

        if (result.IsFailure)
        {
            throw new DeepException(nameof(UpdateTransactionStatus), result.Error);
        }
    }
}
