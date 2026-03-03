using System.Reflection;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.Database;
using Deep.Common.Application.DomainEvents;
using Deep.Common.Application.Inbox;
using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Application.Outbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;

namespace Deep.Common.Application;

public static class ModuleRegistrationHelper
{
    public static IServiceCollection AddDomainEventHandlers(
        this IServiceCollection services,
        Assembly assembly,
        string schema
    )
    {
        Type[] domainEventHandlerTypes = assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract
                && !type.IsInterface
                && type.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)
                    )
            )
            .ToArray();

        List<Type> domainEventHandlerInterfaces = [];

        foreach (Type domainEventHandlerType in domainEventHandlerTypes)
        {
            services.TryAddScoped(domainEventHandlerType);

            Type[] handlerInterfaces = domainEventHandlerType
                .GetInterfaces()
                .Where(i =>
                    i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)
                )
                .ToArray();

            foreach (Type handlerInterface in handlerInterfaces)
            {
                services.TryAddEnumerable(
                    ServiceDescriptor.Scoped(handlerInterface, domainEventHandlerType)
                );

                domainEventHandlerInterfaces.Add(handlerInterface);
            }
        }

        foreach (Type handlerInterface in domainEventHandlerInterfaces.Distinct())
        {
            Type domainEventType = handlerInterface.GetGenericArguments()[0];
            Type closedIdempotentHandlerType = typeof(IdempotentDomainEventHandler<>).MakeGenericType(
                domainEventType
            );

            services.Decorate(
                handlerInterface,
                (inner, sp) =>
                    ActivatorUtilities.CreateInstance(
                        sp,
                        closedIdempotentHandlerType,
                        inner,
                        schema
                    )
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
        Type[] integrationEventHandlerTypes = assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract
                && !type.IsInterface
                && type.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)
                    )
            )
            .ToArray();

        List<Type> integrationEventHandlerInterfaces = [];

        foreach (Type integrationEventHandlerType in integrationEventHandlerTypes)
        {
            services.TryAddScoped(integrationEventHandlerType);

            Type[] handlerInterfaces = integrationEventHandlerType
                .GetInterfaces()
                .Where(i =>
                    i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)
                )
                .ToArray();

            foreach (Type handlerInterface in handlerInterfaces)
            {
                services.TryAddEnumerable(
                    ServiceDescriptor.Scoped(handlerInterface, integrationEventHandlerType)
                );

                integrationEventHandlerInterfaces.Add(handlerInterface);
            }
        }

        foreach (Type handlerInterface in integrationEventHandlerInterfaces.Distinct())
        {
            Type integrationEventType = handlerInterface.GetGenericArguments()[0];
            Type closedIdempotentHandlerType =
                typeof(IdempotentIntegrationEventHandler<>).MakeGenericType(integrationEventType);

            services.Decorate(
                handlerInterface,
                (inner, sp) =>
                    ActivatorUtilities.CreateInstance(
                        sp,
                        closedIdempotentHandlerType,
                        inner,
                        schema
                    )
            );
        }

        return services;
    }

    public static IServiceCollection AddEndpoints(
        this IServiceCollection services,
        Assembly assembly
    )
    {
        services.AddEndpointExtension(assembly);
        return services;
    }

    public static IServiceCollection AddOutboxInterceptor<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddSingleton<IInterceptor>(new InsertOutboxMessagesInterceptor(typeof(TDbContext)));
        return services;
    }

    public static IServiceCollection AddOutboxInboxJobs<TOutboxJob, TInboxJob, TInboxWriter>(
        this IServiceCollection services
    )
        where TOutboxJob : ProcessOutboxJobBase
        where TInboxJob : ProcessInboxJobBase
        where TInboxWriter : InboxWriterBase
    {
        services.AddScoped<TOutboxJob>();
        services.AddScoped<TInboxJob>();
        services.AddScoped<TInboxWriter>();
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
        {
            IMongoClient client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(databaseName);
        });
        services.AddScoped<TContext>();
        return services;
    }

    public static IServiceCollection AddPostgresDbContextWithSchema<TDbContext>(
        this IServiceCollection services,
        string schema
    )
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        services.AddDbContext<TDbContext>(
            (sp, options) =>
            {
                IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
                Postgres.StandardOptions(configuration, schema)(sp, options);
            }
        );
        return services;
    }

    public static void ConfigureConsumers(
        Assembly assembly,
        IRegistrationConfigurator registrationConfigurator
    ) => registrationConfigurator.AddConsumers(assembly);
}
