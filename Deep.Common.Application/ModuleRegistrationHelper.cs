using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.Database;
using Deep.Common.Application.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Reflection;

namespace Deep.Common.Application;


public static class ModuleRegistrationHelper
{
    public static IHostApplicationBuilder AddDomainEventHandlers(this IHostApplicationBuilder builder, Assembly assembly)
    {
        assembly
            .GetTypes()
            .Where(type => typeof(IDomainEventHandler).IsAssignableFrom(type))
            .ToList()
            .ForEach(builder.Services.TryAddScoped);
        return builder;
    }

    public static IHostApplicationBuilder AddEndpoints(this IHostApplicationBuilder builder, Assembly assembly)
    {
        builder.Services.AddEndpoints(assembly);
        return builder;
    }

    public static IHostApplicationBuilder AddDomainEventInterceptor<TDbContext>(
        this IHostApplicationBuilder builder,
        Assembly assembly)
        where TDbContext : DbContext
    {
        builder.Services.AddSingleton<IInterceptor>(sp =>
           new PublishDomainEventsInterceptor(
               sp.GetRequiredService<IServiceScopeFactory>(),
               assembly,
               typeof(TDbContext)));

        return builder;
    }
    public static IHostApplicationBuilder AddMongoDb<TContext>(this IHostApplicationBuilder builder, string connectionName, string databaseName, Action? configureSerializers = null)
        where TContext : class
    {
        builder.AddMongoDBClient(connectionName: connectionName);
        configureSerializers?.Invoke();
        builder.Services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(databaseName);
        });
        builder.Services.AddScoped<TContext>();
        return builder;
    }

    public static IHostApplicationBuilder AddPostgresDbContextWithSchema<TDbContext>(
        this IHostApplicationBuilder builder,
        IConfiguration configuration,
        string schema,
        Action<IServiceCollection, IConfiguration>? additionalInfrastructure = null)
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        builder.Services.AddDbContext<TDbContext>(Postgres.StandardOptions(configuration, schema));
        builder.EnrichNpgsqlDbContext<TDbContext>();
        additionalInfrastructure?.Invoke(builder.Services, configuration);
        return builder;
    }

    public static void ConfigureConsumers(Assembly assembly, IRegistrationConfigurator registrationConfigurator)
    {
        registrationConfigurator.AddConsumers(assembly);
    }
}
