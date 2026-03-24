using Vast.Common.Application.Exceptions;
using Vast.Common.Application.IntegrationEvents;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;
using Vast.Transactions.Application.Features.Transactions;
using Vast.Transactions.Domain.Transaction;
using Vast.Transactions.IntegrationEvents;

namespace Vast.Transactions.Application.IntegrationEventHandlers;

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
            throw new VastException(nameof(UpdateTransactionStatus), result.Error);
        }
    }
}
