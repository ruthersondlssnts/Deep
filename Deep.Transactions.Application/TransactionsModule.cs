using Deep.Common.Api.Endpoints;
using Deep.Common.Database;
using Deep.Common.Messaging;
using Deep.Transactions.Application.Data;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Deep.Transactions.Application;
public static class TransactionsModule
{
    public static IHostApplicationBuilder AddTransactionsModule(
        this IHostApplicationBuilder builder)
    {
        builder.Services.AddDomainEventHandlers();
        builder.Services
            .AddInfrastructure(builder.Configuration)
            .AddEndpoints(AssemblyReference.Assembly);

        // Aspire enrichment belongs HERE
        builder.EnrichNpgsqlDbContext<TransactionsDbContext>();
        return builder;
    }

    private static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services.AddDatabase(configuration);

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services.AddDbContext<TransactionsDbContext>(
            Postgres.StandardOptions(configuration, Schemas.Transactions));

    private static void AddDomainEventHandlers(this IServiceCollection services) =>
       AssemblyReference.Assembly
           .GetTypes()
           .Where(type => type.IsAssignableTo(typeof(IDomainEventHandler)))
           .ToList()
           .ForEach(services.TryAddScoped);

    public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator) =>
         registrationConfigurator.AddConsumers(AssemblyReference.Assembly);
}
