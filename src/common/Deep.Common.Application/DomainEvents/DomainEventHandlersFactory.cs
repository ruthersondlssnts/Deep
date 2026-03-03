using System.Collections.Concurrent;
using System.Reflection;
using Deep.Common.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Common.Application.DomainEvents;

public static class DomainEventHandlersFactory
{
    private static readonly ConcurrentDictionary<string, Type> HandlerServiceTypeDictionary = new();
    private static readonly ConcurrentDictionary<string, Type?> DomainEventTypesDictionary = new();

    public static IEnumerable<IDomainEventHandler> GetHandlers(
        Type domainEventType,
        IServiceProvider serviceProvider,
        Assembly assembly
    )
    {
        Type handlerServiceType = HandlerServiceTypeDictionary.GetOrAdd(
            $"{assembly.GetName().Name}{domainEventType.Name}",
            _ => typeof(IDomainEventHandler<>).MakeGenericType(domainEventType)
        );

        return serviceProvider
            .GetServices(handlerServiceType)
            .Select(domainEventHandler => (domainEventHandler as IDomainEventHandler)!)
            .ToList();
    }

    public static Type? GetDomainEventType(string domainEventTypeName, Assembly assembly)
    {
        return DomainEventTypesDictionary.GetOrAdd(
            $"{assembly.GetName().Name}{domainEventTypeName}",
            _ =>
            {
                Type[] domainEventTypes = assembly
                    .GetTypes()
                    .Where(type =>
                        !type.IsAbstract
                        && !type.IsInterface
                        && type.GetInterfaces().Any(i =>
                            i.IsGenericType
                            && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)
                        )
                    )
                    .SelectMany(type =>
                        type.GetInterfaces()
                            .Where(i =>
                                i.IsGenericType
                                && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)
                            )
                            .Select(i => i.GetGenericArguments()[0])
                    )
                    .Distinct()
                    .ToArray();

                return domainEventTypes.FirstOrDefault(type =>
                    string.Equals(type.Name, domainEventTypeName, StringComparison.Ordinal)
                    || string.Equals(type.FullName, domainEventTypeName, StringComparison.Ordinal)
                );
            }
        );
    }
}
