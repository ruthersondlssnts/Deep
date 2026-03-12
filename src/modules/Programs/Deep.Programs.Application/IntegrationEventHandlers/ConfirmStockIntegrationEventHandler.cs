using Deep.Common.Application.Exceptions;
using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Features.Programs;
using Deep.Transactions.IntegrationEvents;

namespace Deep.Programs.Application.IntegrationEventHandlers;

internal sealed class ConfirmStockIntegrationEventHandler(IRequestBus requestBus)
    : IntegrationEventHandler<ConfirmStockIntegrationEvent>
{
    public override async Task Handle(
        ConfirmStockIntegrationEvent command,
        CancellationToken cancellationToken = default
    )
    {
        Result<ConfirmStock.Response> result = await requestBus.Send<ConfirmStock.Response>(
            new ConfirmStock.Command(command.ProgramId, command.ProductSku, command.Quantity),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new DeepException(nameof(ConfirmStock), result.Error);
        }
    }
}
