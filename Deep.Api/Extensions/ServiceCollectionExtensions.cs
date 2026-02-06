using Deep.Accounts.Application;
using Deep.Accounts.Application.Data;
using Deep.Common.Api.Middleware;
using Deep.Common.Application.Api;
using Deep.Programs.Application;
using Deep.Programs.Application.Data;
using Deep.Transactions.Application;
using Deep.Transactions.Application.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Deep.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApiAndSwagger(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
            options.CustomSchemaIds(t => t.FullName?.Replace("+", "."))
        );
        return services;
    }

    public static IServiceCollection AddExceptionAndProblemDetails(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }

    public static IServiceCollection AddModules(
        this IServiceCollection services,
        string databaseConnectionString,
        string messagingConnectionString
    )
    {
        services
            .AddDapperAndNpgsql(databaseConnectionString)
            .AddCustomMediatR(
                Deep.Programs.Application.AssemblyReference.Assembly,
                Deep.Accounts.Application.AssemblyReference.Assembly,
                Deep.Transactions.Application.AssemblyReference.Assembly
            )
            .AddMassTransit(
                messagingConnectionString,
                [
                    ProgramsModule.ConfigureConsumers,
                    AccountsModule.ConfigureConsumers,
                    TransactionsModule.ConfigureConsumers,
                ]
            );

        services.AddProgramsModule().AddAccountsModule().AddTransactionsModule();

        return services;
    }

    public static IHostApplicationBuilder ApplyAspire(
        this IHostApplicationBuilder builder,
        string sqlConnection,
        string noSqlConnection,
        string amqConnection
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

        return builder;
    }
}
