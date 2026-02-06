// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Common.Domain;

namespace Deep.Common.Application.DomainEvents;

public abstract class DomainEventHandler<TDomainEvent> : IDomainEventHandler, IDomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    public Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken = default) =>
        Handle((TDomainEvent)domainEvent, cancellationToken);

    public abstract Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
