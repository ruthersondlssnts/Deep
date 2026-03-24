using Vast.Common.Application.Exceptions;
using Vast.Common.Application.IntegrationEvents;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;
using Vast.Programs.Application.Features.Programs;
using Vast.Transactions.IntegrationEvents;

namespace Vast.Programs.Application.IntegrationEventHandlers;

internal sealed class ReleaseStockIntegrationEventHandler(IRequestBus requestBus)
    : IntegrationEventHandler<ReleaseStockIntegrationEvent>
{
    public override async Task Handle(
        ReleaseStockIntegrationEvent command,
        CancellationToken cancellationToken = default
    )
    {
        Result<ReleaseStock.Response> result = await requestBus.Send<ReleaseStock.Response>(
            new ReleaseStock.Command(command.ProgramId, command.ProductSku, command.Quantity),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new VastException(nameof(ReleaseStock), result.Error);
        }
    }
}
