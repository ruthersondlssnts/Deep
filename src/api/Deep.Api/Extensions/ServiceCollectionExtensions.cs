using Deep.Accounts.Application;
using Deep.Accounts.Application.Data;
using Deep.Common.Application.Api;
using Deep.Common.Application.Api.Middleware;
using Deep.Common.Application.Auditing;
using Deep.Common.Application.Authentication;
using Deep.Common.Application.Authorization;
using Deep.Programs.Application;
using Deep.Programs.Application.Data;
using Deep.Transactions.Application;
using Deep.Transactions.Application.Data;
using MassTransit;
using Microsoft.OpenApi;

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

            options.AddSecurityDefinition(
                "bearer",
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme.",
                }
            );

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = [],
            });
        });
        return services;
    }

    public static IServiceCollection AddExceptionAndProblemDetails(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddSingleton<IProblemDetailsService, ValidationProblemDetailsWriter>();
        return services;
    }

    public static IServiceCollection AddModules(
        this IServiceCollection services,
        string databaseConnectionString,
        string messagingConnectionString,
        string? redisConnectionString,
        IConfiguration configuration
    )
    {
        services.AddAuditing();

        services
            .AddDapperAndNpgsql(databaseConnectionString)
            .AddCustomMediatR(
                Deep.Programs.Application.AssemblyReference.Assembly,
                Deep.Accounts.Application.AssemblyReference.Assembly,
                Deep.Transactions.Application.AssemblyReference.Assembly
            )
            .AddMassTransit(
                messagingConnectionString,
                redisConnectionString,
                ProgramsModule.ConfigureConsumers,
                AccountsModule.ConfigureConsumers,
                TransactionsModule.ConfigureConsumers
            );

        services.AddAuthenticationInternal();
        services.AddAuthorizationInternal();

        services
            .AddProgramsModule(configuration)
            .AddAccountsModule(configuration)
            .AddTransactionsModule(configuration);

        return services;
    }

    public static IHostApplicationBuilder ApplyAspire(
        this IHostApplicationBuilder builder,
        string sqlConnection,
        string noSqlConnection,
        string amqConnection,
        string? redisConnection = null
    )
    {
        if (!string.IsNullOrWhiteSpace(sqlConnection))
        {
            builder.EnrichNpgsqlDbContext<ProgramsDbContext>();
            builder.EnrichNpgsqlDbContext<AccountsDbContext>();
            builder.EnrichNpgsqlDbContext<TransactionsDbContext>();
        }

        if (!string.IsNullOrWhiteSpace(noSqlConnection))
        {
            builder.AddMongoDBClient(noSqlConnection);
        }

        if (!string.IsNullOrWhiteSpace(amqConnection))
        {
            builder.AddRabbitMQClient(amqConnection);
        }

        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            builder.AddRedisClient(redisConnection);
        }

        return builder;
    }
}
