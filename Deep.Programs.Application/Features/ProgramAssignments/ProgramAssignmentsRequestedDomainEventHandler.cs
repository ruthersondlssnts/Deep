using Deep.Common.Application.DomainEvents;
using Deep.Common.Application.Exceptions;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Domain.Programs;

namespace Deep.Programs.Application.Features.ProgramAssignments;

internal sealed class ProgramAssignmentsRequestedDomainEventHandler(IRequestBus requestBus)
    : DomainEventHandler<ProgramAssignmentsRequestedDomainEvent>
{
    public override async Task Handle(
        ProgramAssignmentsRequestedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<CreateProgramAssignments.Response> result = 
            await requestBus.Send<CreateProgramAssignments.Response>(
                new CreateProgramAssignments.Command(
                    domainEvent.ProgramId,
                    domainEvent.Assignments
                ),
                cancellationToken
            );

        if (result.IsFailure)
        {
            throw new DeepException(
                nameof(ProgramAssignmentsRequestedDomainEventHandler),
                result.Error
            );
        }
    }
}
