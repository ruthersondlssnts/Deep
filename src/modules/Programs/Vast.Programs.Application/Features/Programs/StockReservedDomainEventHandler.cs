using Vast.Common.Application.DomainEvents;
using Vast.Common.Application.IntegrationEvents;
using Vast.Programs.Domain.Programs;
using Vast.Programs.IntegrationEvents;

namespace Vast.Programs.Application.Features.Programs;

internal sealed class StockReservedDomainEventHandler(IEventBus eventBus)
    : DomainEventHandler<StockReservedDomainEvent>
{
    public override async Task Handle(
        StockReservedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    ) =>
        await eventBus.PublishAsync(
            new StockReservedIntegrationEvent(
                Guid.CreateVersion7(),
                DateTime.UtcNow,
                domainEvent.TransactionId,
                domainEvent.ProgramId,
                domainEvent.ProductSku,
                domainEvent.Quantity,
                domainEvent.UnitPrice
            ),
            cancellationToken
        );
}
