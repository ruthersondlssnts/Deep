using Deep.Common.Application.DomainEvents;
using Deep.Common.Application.Exceptions;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Domain.Programs;

namespace Deep.Programs.Application.Features.ProgramAssignments;

internal sealed class ProgramAssignmentsUpdatedDomainEventHandler(IRequestBus requestBus)
    : DomainEventHandler<ProgramAssignmentsUpdatedDomainEvent>
{
    public override async Task Handle(
        ProgramAssignmentsUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<UpdateProgramAssignments.Response> result = 
            await requestBus.Send<UpdateProgramAssignments.Response>(
                new UpdateProgramAssignments.Command(
                    domainEvent.ProgramId,
                    domainEvent.Assignments
                ),
                cancellationToken
            );

        if (result.IsFailure)
        {
            throw new DeepException(
                nameof(ProgramAssignmentsUpdatedDomainEventHandler),
                result.Error
            );
        }
    }
}
