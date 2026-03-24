namespace Vast.Common.Application.IntegrationEvents;

public abstract class IntegrationEventHandler<TIntegrationEvent>
    : IIntegrationEventHandler,
        IIntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    public Task Handle(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    ) => Handle((TIntegrationEvent)integrationEvent, cancellationToken);

    public abstract Task Handle(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    );
}
