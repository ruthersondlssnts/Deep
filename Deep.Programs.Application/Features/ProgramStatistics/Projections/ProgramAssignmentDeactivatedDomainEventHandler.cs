using Deep.Common.Application.Exceptions;
using Deep.Common.Application.Messaging;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Features.Programs;
using Deep.Programs.Domain.ProgramAssignments;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Deep.Programs.Application.Features.ProgramStatistics.Projections;

internal sealed class ProgramAssignmentDeactivatedDomainEventHandler(
   IRequestBus requestBus)
    : DomainEventHandler<ProgramAssignmentDeactivatedDomainEvent>
{
    public override async Task Handle(
        ProgramAssignmentDeactivatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var program = await requestBus.Send<GetProgram.Response>(new GetProgram.Query(domainEvent.ProgramId), cancellationToken);

        if (program == null)
            return;

        var result = await requestBus.Send<UpsertProgramStatistic.Response>(
           new UpsertProgramStatistic.Command(
                ProgramId: program.Value.Id,
                TotalCoordinators: program.Value.Assignments.Count(a => a.Role == Role.Coordinator),
                TotalBrandAmbassadors: program.Value.Assignments.Count(a => a.Role == Role.BrandAmbassador)
           ));

        if (result.IsFailure)
            throw new DeepException(
                nameof(UpsertProgramStatistic),
                result.Error);
    }
}