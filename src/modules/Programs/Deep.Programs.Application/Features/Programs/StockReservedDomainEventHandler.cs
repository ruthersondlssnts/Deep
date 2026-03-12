using Deep.Common.Application.DomainEvents;
using Deep.Common.Application.IntegrationEvents;
using Deep.Programs.Domain.Programs;
using Deep.Programs.IntegrationEvents;

namespace Deep.Programs.Application.Features.Programs;

internal sealed class StockReservedDomainEventHandler(
    IEventBus eventBus
) : DomainEventHandler<StockReservedDomainEvent>
{
    public override async Task Handle(
        StockReservedDomainEvent domainEvent,
        CancellationToken cancellationToken = default) =>
        await eventBus.PublishAsync(
            new StockReservedIntegrationEvent(
                Guid.CreateVersion7(),
                DateTime.UtcNow,
                domainEvent.TransactionId,
                domainEvent.ProgramId,
                domainEvent.ProductSku,
                domainEvent.Quantity,
                domainEvent.UnitPrice),
            cancellationToken);
}
