using Vast.Common.Application.Exceptions;
using Vast.Common.Application.IntegrationEvents;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;
using Vast.Transactions.Application.Features.Transactions;
using Vast.Transactions.Domain.Transaction;
using Vast.Transactions.IntegrationEvents;

namespace Vast.Transactions.Application.IntegrationEventHandlers;

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
            throw new VastException(nameof(UpdateTransactionStatus), result.Error);
        }
    }
}
