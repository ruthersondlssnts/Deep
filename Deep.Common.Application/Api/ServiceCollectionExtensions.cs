using Deep.Common.Application.Api;
using Deep.Common.Application.Dapper;
using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Application.SimpleMediatR;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace Deep.Common.Application.Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDapperAndNpgsql(this IServiceCollection services, string databaseConnectionString)
    {
        var npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
        services.TryAddSingleton(npgsqlDataSource);
        services.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();
        return services;
    }

    public static IServiceCollection AddCustomMediatR(this IServiceCollection services, params System.Reflection.Assembly[] assemblies)
    {
        services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);
        services.AddScoped<IRequestBus, RequestBus>();
        services.AddRequestHandlers(assemblies);
        services.AddRequestPipelines(
            typeof(ValidationPipelineBehavior<,>),
            typeof(RequestLoggingPipelineBehavior<,>),
            typeof(ExceptionHandlingPipelineBehavior<,>));
        services.TryAddSingleton<IEventBus, IntegrationEvents.EventBus>();
        return services;
    }

    public static IServiceCollection AddMassTransit(
        this IServiceCollection services,
        string mqConnectionString,
        params Action<IRegistrationConfigurator>[] configureConsumers)
    {
        services.AddMassTransit(configurator =>
        {
            foreach (var configureConsumer in configureConsumers)
            {
                configureConsumer(configurator);
            }

            configurator.SetKebabCaseEndpointNameFormatter();

            configurator.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = mqConnectionString;
                cfg.Host(new Uri(connectionString));
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }



}
