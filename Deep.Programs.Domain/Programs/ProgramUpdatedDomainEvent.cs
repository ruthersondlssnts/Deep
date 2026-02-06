// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Common.Domain;

namespace Deep.Programs.Domain.Programs;

public sealed class ProgramUpdatedDomainEvent(
    Guid programId) : DomainEvent
{
    public Guid ProgramId { get; } = programId;
}
