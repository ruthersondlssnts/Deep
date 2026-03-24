using Vast.Common.Application.DomainEvents;
using Vast.Common.Application.Exceptions;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;
using Vast.Programs.Application.Features.Programs;
using Vast.Programs.Domain.Programs;

namespace Vast.Programs.Application.Features.ProgramStatistics.Projections;

internal sealed class ProgramCreatedDomainEventHandler(IRequestBus requestBus)
    : DomainEventHandler<ProgramCreatedDomainEvent>
{
    public override async Task Handle(
        ProgramCreatedDomainEvent domainEvent,
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
            throw new VastException(nameof(UpsertProgramStatistic), result.Error);
        }
    }
}
