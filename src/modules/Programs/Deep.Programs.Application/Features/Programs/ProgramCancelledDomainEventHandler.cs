using Deep.Common.Application.DomainEvents;
using Deep.Common.Application.IntegrationEvents;
using Deep.Programs.Domain.Programs;
using Deep.Programs.IntegrationEvents;

namespace Deep.Programs.Application.Features.Programs;

internal sealed class ProgramCancelledDomainEventHandler(IEventBus eventBus)
    : DomainEventHandler<ProgramCancelledDomainEvent>
{
    public override async Task Handle(
        ProgramCancelledDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new ProgramCancelledIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredAtUtc,
                domainEvent.ProgramId,
                domainEvent.Reason),
            cancellationToken);
    }
}
