using Deep.Common.Api.Middleware;
using Deep.Common.Application.Dapper;
using Deep.Common.Application.EventBus;
using Deep.Common.Application.SimpleMediatR;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace Deep.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApiAndSwagger(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(t => t.FullName?.Replace("+", "."));
        });
        return services;
    }

    public static IServiceCollection AddExceptionAndProblemDetails(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }

    public static IServiceCollection AddDapperAndNpgsql(this IServiceCollection services, string databaseConnectionString)
    {
        var npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
        services.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.TryAddSingleton(npgsqlDataSource);
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
        services.TryAddSingleton<IEventBus, EventBus>();
        return services;
    }

    public static IServiceCollection AddMassTransit(this IServiceCollection services, params Action<IRegistrationConfigurator>[] configureConsumers)
    {
        services.AddMassTransit(configurator =>
        {
            foreach (var configureConsumer in configureConsumers)
            {
                configureConsumer(configurator);
            }
            configurator.SetKebabCaseEndpointNameFormatter();
            configurator.UsingInMemory((context, config) =>
            {
                config.ConfigureEndpoints(context);
            });
        });
        return services;
    }
}
