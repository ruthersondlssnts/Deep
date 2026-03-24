using System.Collections.Concurrent;
using System.Reflection;
using Vast.Common.Application.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Vast.Common.Application.Inbox;

public static class IntegrationEventHandlersFactory
{
    private static readonly ConcurrentDictionary<string, Type> HandlerServiceTypeDictionary = new();
    private static readonly ConcurrentDictionary<string, Type?> IntegrationEventTypesDictionary =
        new();

    public static IEnumerable<IIntegrationEventHandler> GetHandlers(
        Type integrationEventType,
        IServiceProvider serviceProvider,
        Assembly assembly
    )
    {
        Type handlerServiceType = HandlerServiceTypeDictionary.GetOrAdd(
            $"{assembly.GetName().Name}{integrationEventType.Name}",
            _ => typeof(IIntegrationEventHandler<>).MakeGenericType(integrationEventType)
        );

        return serviceProvider
            .GetServices(handlerServiceType)
            .Select(handler => (handler as IIntegrationEventHandler)!)
            .ToList();
    }

    public static Type? GetIntegrationEventType(
        string integrationEventTypeName,
        Assembly assembly
    ) =>
        IntegrationEventTypesDictionary.GetOrAdd(
            $"{assembly.GetName().Name}{integrationEventTypeName}",
            _ =>
            {
                Type[] integrationEventTypes = assembly
                    .GetTypes()
                    .Where(type =>
                        !type.IsAbstract
                        && !type.IsInterface
                        && type.GetInterfaces()
                            .Any(i =>
                                i.IsGenericType
                                && i.GetGenericTypeDefinition()
                                    == typeof(IIntegrationEventHandler<>)
                            )
                    )
                    .SelectMany(type =>
                        type.GetInterfaces()
                            .Where(i =>
                                i.IsGenericType
                                && i.GetGenericTypeDefinition()
                                    == typeof(IIntegrationEventHandler<>)
                            )
                            .Select(i => i.GetGenericArguments()[0])
                    )
                    .Distinct()
                    .ToArray();

                return integrationEventTypes.FirstOrDefault(type =>
                    string.Equals(type.Name, integrationEventTypeName, StringComparison.Ordinal)
                    || string.Equals(
                        type.FullName,
                        integrationEventTypeName,
                        StringComparison.Ordinal
                    )
                );
            }
        );
}
