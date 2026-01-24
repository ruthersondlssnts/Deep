using MassTransit;

namespace Deep.Common.Application.EventBus;

public sealed class EventBus(IBus bus) : IEventBus
{
    public async Task PublishAsync<TIntegrationEvent>(
        TIntegrationEvent integrationEvent, 
        CancellationToken cancellationToken = default) 
        where TIntegrationEvent : IIntegrationEvent
    {
        await bus.Publish(integrationEvent, cancellationToken);
    }
}