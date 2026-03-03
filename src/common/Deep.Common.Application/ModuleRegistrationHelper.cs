using System.Reflection;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.Database;
using Deep.Common.Application.DomainEvents;
using Deep.Common.Application.Inbox;
using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Application.Outbox;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

    public static IServiceCollection AddEndpoints(
        this IServiceCollection services,
        Assembly assembly
    )
    {
        services.AddEndpointExtension(assembly);
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
            sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName)
        );
        services.AddScoped<TContext>();
        return services;
    }

    public static IServiceCollection AddPostgresDbContext<TDbContext>(
        this IServiceCollection services,
        string schema,
        IConfiguration configuration
    )
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        services.AddDbContext<TDbContext>(
            (sp, options) => Postgres.ConfigureOptions(options, configuration, schema, sp)
        );
        return services;
    }

    public static void ConfigureConsumers(
        Assembly assembly,
        IRegistrationConfigurator registrationConfigurator
    ) => registrationConfigurator.AddConsumers(assembly);
}
