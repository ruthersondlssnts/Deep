using Deep.Accounts.Application.Data;
using Deep.Common.Api.Middleware;
using Deep.Common.Application.Dapper;
using Deep.Common.Application.EventBus;
using Deep.Common.Application.SimpleMediatR;
using Deep.Programs.Application.Data;
using Deep.Transactions.Application.Data;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
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
        services.TryAddSingleton<IEventBus, EventBus>();
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

    public static IHostApplicationBuilder ApplyAspire(
     this IHostApplicationBuilder builder,
     string sqlConnection,
     string noSqlConnection,
     string amqConnection)
    {
        builder.EnrichNpgsqlDbContext<ProgramsDbContext>();
        builder.EnrichNpgsqlDbContext<AccountsDbContext>();
        builder.EnrichNpgsqlDbContext<TransactionsDbContext>();

        if (!string.IsNullOrWhiteSpace(noSqlConnection))
            builder.AddMongoDBClient(noSqlConnection);

        if (!string.IsNullOrWhiteSpace(amqConnection))
            builder.AddRabbitMQClient(amqConnection);

        return builder;
    }
}
