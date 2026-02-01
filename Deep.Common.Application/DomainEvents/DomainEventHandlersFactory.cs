using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

namespace Deep.Common.Application.DomainEvents;

public static class DomainEventHandlersFactory
{
    private static readonly ConcurrentDictionary<string, Type[]> HandlersDictionary = new();

    public static IEnumerable<IDomainEventHandler> GetHandlers(
        Type domainEventType,
        IServiceProvider serviceProvider,
        Assembly assembly)
    {
        var domainEventHandlerTypes = HandlersDictionary.GetOrAdd(
            $"{assembly.GetName().Name}{domainEventType.Name}",
            _ => assembly.GetTypes()
                .Where(handlerType => handlerType.IsAssignableTo(typeof(IDomainEventHandler<>).MakeGenericType(domainEventType)))
                .ToArray());

        return domainEventHandlerTypes
            .Select(serviceProvider.GetRequiredService)
            .Select(domainEventHandler => (domainEventHandler as IDomainEventHandler)!)
            .ToList();
    }

}