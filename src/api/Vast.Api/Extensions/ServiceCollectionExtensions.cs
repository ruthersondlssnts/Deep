using Vast.Accounts.Application;
using Vast.Accounts.Application.Data;
using Vast.Common.Application.Api;
using Vast.Common.Application.Api.Middleware;
using Vast.Common.Application.Auditing;
using Vast.Common.Application.Authentication;
using Vast.Common.Application.Authorization;
using Vast.Programs.Application;
using Vast.Programs.Application.Data;
using Vast.Transactions.Application;
using Vast.Transactions.Application.Data;
using MassTransit;
using Microsoft.OpenApi;

namespace Vast.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApiAndSwagger(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(t => t.FullName?.Replace("+", ".", StringComparison.Ordinal));

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
                Vast.Programs.Application.AssemblyReference.Assembly,
                Vast.Accounts.Application.AssemblyReference.Assembly,
                Vast.Transactions.Application.AssemblyReference.Assembly
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
