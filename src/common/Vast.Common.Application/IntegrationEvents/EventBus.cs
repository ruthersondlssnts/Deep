using MassTransit;

namespace Vast.Common.Application.IntegrationEvents;

public sealed class EventBus(IBus bus) : IEventBus
{
    public async Task PublishAsync<TIntegrationEvent>(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
        where TIntegrationEvent : IIntegrationEvent =>
        await bus.Publish(integrationEvent, cancellationToken);
}
