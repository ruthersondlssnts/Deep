using Deep.Common.Application.Exceptions;
using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Features.Programs;
using Deep.Transactions.IntegrationEvents;

namespace Deep.Programs.Application.IntegrationEventHandlers;

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
            throw new DeepException(nameof(ReleaseStock), result.Error);
        }
    }
}
