namespace Deep.Common.Application.IntegrationEvents;

public interface IEventBus
{
    Task PublishAsync<TIntegrationEvent>(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TIntegrationEvent : IIntegrationEvent;
}
