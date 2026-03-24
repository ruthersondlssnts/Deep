using Vast.Common.Application.Exceptions;
using Vast.Common.Application.IntegrationEvents;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;
using Vast.Programs.Application.Features.Programs;
using Vast.Transactions.IntegrationEvents;

namespace Vast.Programs.Application.IntegrationEventHandlers;

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
            throw new VastException(nameof(ConfirmStock), result.Error);
        }
    }
}
