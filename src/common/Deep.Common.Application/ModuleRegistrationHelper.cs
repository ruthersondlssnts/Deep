using System.Reflection;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.Database;
using Deep.Common.Application.DomainEvents;
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
        Assembly assembly
    )
    {
        assembly
            .GetTypes()
            .Where(type => typeof(IDomainEventHandler).IsAssignableFrom(type))
            .ToList()
            .ForEach(services.TryAddScoped);
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

    public static IServiceCollection AddDomainEventInterceptor<TDbContext>(
        this IServiceCollection services,
        Assembly assembly
    )
        where TDbContext : DbContext
    {
        services.AddSingleton<IInterceptor>(sp => new PublishDomainEventsInterceptor(
            sp.GetRequiredService<IServiceScopeFactory>(),
            assembly,
            typeof(TDbContext)
        ));
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
