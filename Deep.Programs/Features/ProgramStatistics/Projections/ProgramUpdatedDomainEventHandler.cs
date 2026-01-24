using Deep.Common.Domain;
using Deep.Common.Exceptions;
using Deep.Common.Messaging;
using Deep.Common.SimpleMediatR;
using Deep.Programs.Domain.Programs;
using Deep.Programs.Features.Programs;
using Deep.Programs.Features.ProgramStatistics;

namespace Deep.Programs.Features.ProgramStatistics.Projections;

internal sealed class ProgramUpdatedDomainEventHandler(
   IRequestBus requestBus)
    : DomainEventHandler<ProgramUpdatedDomainEvent>
{
    public override async Task Handle(ProgramUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var program = await requestBus.Send<GetProgram.Response>(new GetProgram.Query(domainEvent.ProgramId), cancellationToken);

        if (program == null)
        {
            return;
        }

        var result = await requestBus.Send<UpsertProgramStatistic.Response>(
           new UpsertProgramStatistic.Command(
               program.Value.Id,
               program.Value.Name,
               program.Value.Description,
               program.Value.ProgramStatus,
               program.Value.StartsAtUtc,
               program.Value.EndsAtUtc,
               program.Value.OwnerId,
               program.Value.OwnerName,
               program.Value.Assignments.Count(a => a.Role == Role.Coordinator),
               program.Value.Assignments.Count(a => a.Role == Role.BrandAmbassador)
           ));

        if (result.IsFailure)
            throw new DeepException(
                nameof(UpsertProgramStatistic),
                result.Error);
    }
}