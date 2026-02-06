// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Common.Application.DomainEvents;
using Deep.Common.Application.Exceptions;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Features.Programs;
using Deep.Programs.Domain.Programs;

namespace Deep.Programs.Application.Features.ProgramStatistics.Projections;

internal sealed class ProgramCreatedDomainEventHandler(
   IRequestBus requestBus)
    : DomainEventHandler<ProgramCreatedDomainEvent>
{
    public override async Task Handle(ProgramCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
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
               program.Value.Assignments.Count(a => a.RoleName == RoleNames.Coordinator),
               program.Value.Assignments.Count(a => a.RoleName == RoleNames.BrandAmbassador)
           ));

        if (result.IsFailure)
            throw new DeepException(
                nameof(UpsertProgramStatistic),
                result.Error);
    }
}
