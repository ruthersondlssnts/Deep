using Deep.Common.Application.Exceptions;
using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Features.Programs;
using Deep.Transactions.IntegrationEvents;

namespace Deep.Programs.Application.IntegrationEventHandlers;

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
            throw new DeepException(nameof(ReserveStock), result.Error);
        }
    }
}
