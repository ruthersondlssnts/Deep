using System.Reflection;
using Vast.Common.Application.Api.Endpoints;
using Vast.Common.Application.Database;
using Vast.Common.Application.DomainEvents;
using Vast.Common.Application.Inbox;
using Vast.Common.Application.IntegrationEvents;
using Vast.Common.Application.Outbox;
using MassTransit;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Vast.Common.Application;

public static class ModuleRegistrationHelper
{
    public static IServiceCollection AddEndpoints(
        this IServiceCollection services,
        Assembly assembly
    )
    {
        services.AddEndpointExtension(assembly);
        return services;
    }

    public static IServiceCollection AddMongoDb<TContext>(
        this IServiceCollection services,
        string databaseName,
        Action? configureSerializers = null
    )
        where TContext : class
    {
        configureSerializers?.Invoke();
        services.AddSingleton<IMongoDatabase>(sp =>
            sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName)
        );
        services.AddScoped<TContext>();
        return services;
    }

    public static IServiceCollection AddPostgresDbContext<TDbContext, TOutboxInterceptor>(
        this IServiceCollection services,
        string schema,
        IConfiguration configuration
    )
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
        where TOutboxInterceptor : IInterceptor
    {
        services.AddDbContext<TDbContext>(
            (sp, options) =>
                Postgres.ConfigureOptions<TOutboxInterceptor>(options, configuration, schema, sp)
        );

        return services;
    }

    public static IServiceCollection AddDomainEventHandlers(
        this IServiceCollection services,
        Assembly assembly,
        string schema
    )
    {
        services.Scan(scan =>
            scan.FromAssemblies(assembly)
                .AddClasses(
                    classes => classes.AssignableTo(typeof(IDomainEventHandler<>)),
                    publicOnly: false
                )
                .As(type =>
                    type.GetInterfaces()
                        .Where(i =>
                            i.IsGenericType
                            && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)
                        )
                )
                .WithScopedLifetime()
        );

        Type[] handlerInterfaces = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces())
            .Where(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)
            )
            .Distinct()
            .ToArray();

        foreach (Type handlerInterface in handlerInterfaces)
        {
            Type domainEventType = handlerInterface.GetGenericArguments()[0];
            Type closedIdempotentType = typeof(IdempotentDomainEventHandler<>).MakeGenericType(
                domainEventType
            );

            services.Decorate(
                handlerInterface,
                (inner, sp) =>
                    ActivatorUtilities.CreateInstance(sp, closedIdempotentType, inner, schema)
            );
        }

        return services;
    }

    public static IServiceCollection AddIntegrationEventHandlers(
        this IServiceCollection services,
        Assembly assembly,
        string schema
    )
    {
        services.Scan(scan =>
            scan.FromAssemblies(assembly)
                .AddClasses(
                    classes => classes.AssignableTo(typeof(IIntegrationEventHandler<>)),
                    publicOnly: false
                )
                .As(type =>
                    type.GetInterfaces()
                        .Where(i =>
                            i.IsGenericType
                            && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)
                        )
                )
                .WithScopedLifetime()
        );

        Type[] handlerInterfaces = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces())
            .Where(i =>
                i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)
            )
            .Distinct()
            .ToArray();

        foreach (Type handlerInterface in handlerInterfaces)
        {
            Type integrationEventType = handlerInterface.GetGenericArguments()[0];
            Type closedIdempotentType = typeof(IdempotentIntegrationEventHandler<>).MakeGenericType(
                integrationEventType
            );

            services.Decorate(
                handlerInterface,
                (inner, sp) =>
                    ActivatorUtilities.CreateInstance(sp, closedIdempotentType, inner, schema)
            );
        }

        return services;
    }

    public static void ConfigureConsumers(
        Assembly assembly,
        IRegistrationConfigurator registrationConfigurator,
        Type moduleConsumerType
    )
    {
        Type[] integrationEventHandlerTypes = assembly
            .GetTypes()
            .Where(type =>
                type.IsAssignableTo(typeof(IIntegrationEventHandler))
                && !type.IsAbstract
                && !type.IsInterface
            )
            .ToArray();

        foreach (Type integrationEventHandlerType in integrationEventHandlerTypes)
        {
            Type integrationEventType = integrationEventHandlerType
                .GetInterfaces()
                .Single(@interface =>
                    @interface.IsGenericType
                    && @interface.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)
                )
                .GetGenericArguments()
                .Single();

            Type consumerType = moduleConsumerType.MakeGenericType(integrationEventType);

            registrationConfigurator.AddConsumer(consumerType);
        }
    }
}
