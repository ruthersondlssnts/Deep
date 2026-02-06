// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Common.Application.DomainEvents;
using Deep.Programs.Domain.Programs;

namespace Deep.Programs.Application.Features.Programs;

internal sealed class ProgramCreatedNotificationHandler()
    : DomainEventHandler<ProgramCreatedDomainEvent>
{
    public override async Task Handle(ProgramCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {

    }
}
