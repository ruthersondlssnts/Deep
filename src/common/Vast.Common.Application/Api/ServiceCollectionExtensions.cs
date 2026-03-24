using Vast.Common.Application.Api;
using Vast.Common.Application.Dapper;
using Vast.Common.Application.IntegrationEvents;
using Vast.Common.Application.SimpleMediatR;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace Vast.Common.Application.Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDapperAndNpgsql(
        this IServiceCollection services,
        string databaseConnectionString
    )
    {
        NpgsqlDataSource npgsqlDataSource = new NpgsqlDataSourceBuilder(
            databaseConnectionString
        ).Build();
        services.TryAddSingleton(npgsqlDataSource);
        services.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();
        return services;
    }

    public static IServiceCollection AddCustomMediatR(
        this IServiceCollection services,
        params System.Reflection.Assembly[] assemblies
    )
    {
        services.AddScoped<IRequestBus, RequestBus>();
        services.AddRequestHandlers(assemblies);
        services.AddRequestPipelines(
            typeof(RequestLoggingPipelineBehavior<,>),
            typeof(ExceptionHandlingPipelineBehavior<,>)
        );
        services.TryAddSingleton<IEventBus, IntegrationEvents.EventBus>();
        return services;
    }

    public static IServiceCollection AddMassTransit(
        this IServiceCollection services,
        string mqConnectionString,
        string? redisConnectionString,
        params Action<IRegistrationConfigurator, string?>[] configureConsumers
    )
    {
        services.AddMassTransit(configurator =>
        {
            foreach (
                Action<IRegistrationConfigurator, string?> configureConsumer in configureConsumers
            )
            {
                configureConsumer(configurator, redisConnectionString);
            }

            configurator.SetKebabCaseEndpointNameFormatter();

            configurator.UsingRabbitMq(
                (context, cfg) =>
                {
                    cfg.Host(new Uri(mqConnectionString));
                    cfg.ConfigureEndpoints(context);
                }
            );
        });

        return services;
    }
}
