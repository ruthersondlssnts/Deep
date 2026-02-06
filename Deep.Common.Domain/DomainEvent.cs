// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Deep.Common.Domain;

public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; init; }
    public DateTime OccurredAtUtc { get; init; }

    protected DomainEvent()
    {
        Id = Guid.CreateVersion7();
        OccurredAtUtc = DateTime.UtcNow;
    }

    protected DomainEvent(Guid id, DateTime occurredAtUtc)
    {
        Id = id;
        OccurredAtUtc = occurredAtUtc;
    }
}
