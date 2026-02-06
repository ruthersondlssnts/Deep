using System.Reflection;
using Deep.Common.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Common.Application.DomainEvents;

public class PublishDomainEventsInterceptor(
    IServiceScopeFactory serviceScopeFactory,
    Assembly assembly,
    Type dbContextType
) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        if (
            eventData.Context is null
            || !dbContextType.IsAssignableFrom(eventData.Context.GetType())
        )
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        await PublishDomainEventsAsync(eventData.Context, cancellationToken);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task PublishDomainEventsAsync(
        DbContext context,
        CancellationToken cancellationToken
    )
    {
        var domainEvents = context
            .ChangeTracker.Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var domainEvents = entity.GetDomainEvents();
                entity.ClearDomainEvents();
                return domainEvents;
            })
            .ToList();

        foreach (var domainEvent in domainEvents)
            await PublishDomainEvent(domainEvent);
    }

    private async Task PublishDomainEvent(IDomainEvent domainEvent)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var domainEventHandlers = DomainEventHandlersFactory.GetHandlers(
            domainEvent.GetType(),
            scope.ServiceProvider,
            assembly
        );

        foreach (var domainEventHandler in domainEventHandlers)
        {
            await domainEventHandler.Handle(domainEvent);
        }
    }
}
