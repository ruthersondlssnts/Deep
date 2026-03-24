using Vast.Common.Application.DomainEvents;
using Vast.Common.Application.Exceptions;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;
using Vast.Programs.Application.Features.Programs;
using Vast.Programs.Domain.ProgramAssignments;
using MongoDB.Driver;

namespace Vast.Programs.Application.Features.ProgramStatistics.Projections;

internal sealed class ProgramAssignmentDeactivatedDomainEventHandler(IRequestBus requestBus)
    : DomainEventHandler<ProgramAssignmentDeactivatedDomainEvent>
{
    public override async Task Handle(
        ProgramAssignmentDeactivatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<GetProgram.Response> program = await requestBus.Send<GetProgram.Response>(
            new GetProgram.Query(domainEvent.ProgramId),
            cancellationToken
        );

        if (program == null)
        {
            return;
        }

        Result<UpsertProgramStatistic.Response> result =
            await requestBus.Send<UpsertProgramStatistic.Response>(
                new UpsertProgramStatistic.Command(
                    ProgramId: program.Value.Id,
                    TotalCoordinators: program.Value.Assignments.Count(a =>
                        a.RoleName == RoleNames.Coordinator
                    ),
                    TotalBrandAmbassadors: program.Value.Assignments.Count(a =>
                        a.RoleName == RoleNames.BrandAmbassador
                    )
                ),
                cancellationToken
            );

        if (result.IsFailure)
        {
            throw new VastException(nameof(UpsertProgramStatistic), result.Error);
        }
    }
}
