using Vast.Common.Application.Exceptions;
using Vast.Common.Application.IntegrationEvents;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;
using Vast.Programs.Application.Features.Programs;
using Vast.Transactions.IntegrationEvents;

namespace Vast.Programs.Application.IntegrationEventHandlers;

internal sealed class ReserveStockIntegrationEventHandler(IRequestBus requestBus)
    : IntegrationEventHandler<TransactionCreatedIntegrationEvent>
{
    public override async Task Handle(
        TransactionCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<ReserveStock.Response> result = await requestBus.Send<ReserveStock.Response>(
            new ReserveStock.Command(
                integrationEvent.TransactionId,
                integrationEvent.ProgramId,
                integrationEvent.ProductSku,
                integrationEvent.Quantity
            ),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new VastException(nameof(ReserveStock), result.Error);
        }
    }
}
