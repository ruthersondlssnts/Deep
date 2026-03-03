namespace Deep.Common.Application.IntegrationEvents;

public interface IIntegrationEventHandler
{
    Task Handle(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}

public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    Task Handle(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
