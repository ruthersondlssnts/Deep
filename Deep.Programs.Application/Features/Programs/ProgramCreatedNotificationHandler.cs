using Dapper;
using Deep.Common.Application.Messaging;
using Deep.Programs.Domain.Programs;

namespace Deep.Programs.Application.Features.Programs;

internal sealed class ProgramCreatedNotificationHandler()
    : DomainEventHandler<ProgramCreatedDomainEvent>
{
    public override async Task Handle(ProgramCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        
    }
}