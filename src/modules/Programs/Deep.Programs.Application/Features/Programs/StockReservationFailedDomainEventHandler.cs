using Deep.Common.Application.DomainEvents;
using Deep.Common.Application.IntegrationEvents;
using Deep.Programs.Domain.Programs;
using Deep.Programs.IntegrationEvents;

namespace Deep.Programs.Application.Features.Programs;

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
