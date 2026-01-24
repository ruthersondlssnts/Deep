namespace Deep.Common.EventBus;

public interface IEventBus
{
    Task PublishAsync<TIntegrationEvent>(
        TIntegrationEvent integrationEvent, 
        CancellationToken cancellationToken = default)
        where TIntegrationEvent : IIntegrationEvent;
}