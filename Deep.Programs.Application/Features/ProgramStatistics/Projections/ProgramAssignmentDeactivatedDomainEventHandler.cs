using Dapper;
using Deep.Common.Domain;
using Deep.Common.Exceptions;
using Deep.Common.Messaging;
using Deep.Common.SimpleMediatR;
using Deep.Programs.Data;
using Deep.Programs.Domain.ProgramAssignments;
using Deep.Programs.Domain.Programs;
using Deep.Programs.Application.Features.Programs;
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