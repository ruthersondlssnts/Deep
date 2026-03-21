using Deep.Common.Application.DomainEvents;
using Deep.Common.Application.Exceptions;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Features.Programs;
using Deep.Programs.Domain.Programs;

namespace Deep.Programs.Application.Features.ProgramStatistics.Projections;

internal sealed class ProgramUpdatedDomainEventHandler(IRequestBus requestBus)
    : DomainEventHandler<ProgramUpdatedDomainEvent>
{
    public override async Task Handle(
        ProgramUpdatedDomainEvent domainEvent,
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
                    program.Value.Id,
                    program.Value.Name,
                    program.Value.Description,
                    program.Value.ProgramStatus,
                    program.Value.StartsAtUtc,
                    program.Value.EndsAtUtc,
                    program.Value.OwnerId,
                    program.Value.OwnerName,
                    program.Value.Assignments.Count(a => a.RoleName == RoleNames.Coordinator),
                    program.Value.Assignments.Count(a => a.RoleName == RoleNames.BrandAmbassador)
                ),
                cancellationToken
            );

        if (result.IsFailure)
        {
            throw new DeepException(nameof(UpsertProgramStatistic), result.Error);
        }
    }
}
