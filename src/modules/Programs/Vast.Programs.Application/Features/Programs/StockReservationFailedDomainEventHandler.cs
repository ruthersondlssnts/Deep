using Vast.Common.Application.DomainEvents;
using Vast.Common.Application.IntegrationEvents;
using Vast.Programs.Domain.Programs;
using Vast.Programs.IntegrationEvents;

namespace Vast.Programs.Application.Features.Programs;

internal sealed class StockReservationFailedDomainEventHandler(IEventBus eventBus)
    : DomainEventHandler<StockReservationFailedDomainEvent>
{
    public override async Task Handle(
        StockReservationFailedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    ) =>
        await eventBus.PublishAsync(
            new StockReservationFailedIntegrationEvent(
                Guid.CreateVersion7(),
                DateTime.UtcNow,
                domainEvent.TransactionId,
                domainEvent.ProgramId,
                domainEvent.ProductSku,
                domainEvent.Quantity,
                domainEvent.Reason
            ),
            cancellationToken
        );
}
