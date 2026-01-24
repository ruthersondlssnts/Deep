using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Deep.Common.Domain;

public abstract class Entity
{
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.ToList();

    private readonly List<IDomainEvent> _domainEvents = new();

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
