using Deep.Common.Application.Exceptions;
using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Transactions.Application.Features.Transactions;
using Deep.Transactions.Domain.Transaction;
using Deep.Transactions.IntegrationEvents;

namespace Deep.Transactions.Application.IntegrationEventHandlers;

internal sealed class TransactionFailedIntegrationEventHandler(IRequestBus requestBus)
    : IntegrationEventHandler<TransactionFailedIntegrationEvent>
{
    public override async Task Handle(
        TransactionFailedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<UpdateTransactionStatus.Response> result =
            await requestBus.Send<UpdateTransactionStatus.Response>(
                new UpdateTransactionStatus.Command(
                    integrationEvent.TransactionId,
                    TransactionStatus.Failed,
                    FailureReason: integrationEvent.Reason
                ),
                cancellationToken
            );

        if (result.IsFailure)
        {
            throw new DeepException(nameof(UpdateTransactionStatus), result.Error);
        }
    }
}
