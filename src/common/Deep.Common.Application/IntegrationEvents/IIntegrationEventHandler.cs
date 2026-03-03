namespace Deep.Common.Application.IntegrationEvents;

/// <summary>
/// Interface for integration event handlers used with the inbox pattern.
/// </summary>
public interface IIntegrationEventHandler
{
    Task Handle(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Typed interface for integration event handlers.
/// </summary>
public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    Task Handle(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
