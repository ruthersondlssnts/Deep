using Deep.Common.Application;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Deep.Common.Application.Outbox;
using Deep.Transactions.Application.BackgroundJobs;
using Deep.Transactions.Application.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Transactions.Application;

public static class TransactionsModule
{
    public const string ModuleName = "Transactions";

    public static IServiceCollection AddTransactionsModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddValidation()
            .AddDomainEventHandlers(AssemblyReference.Assembly, Schemas.Transactions)
            .AddIntegrationEventHandlers(AssemblyReference.Assembly, Schemas.Transactions)
            .AddPostgresDbContext<TransactionsDbContext>(Schemas.Transactions, configuration)
            .AddEndpoints(AssemblyReference.Assembly)
            .AddOutboxInboxJobs<
                TransactionsProcessOutboxJob,
                TransactionsProcessInboxJob,
                TransactionsInboxWriter
            >();

        services.Configure<OutboxOptions>(configuration.GetSection("Transactions:Outbox"));
        services.Configure<InboxOptions>(configuration.GetSection("Transactions:Inbox"));

        return services;
    }

    public static void ConfigureConsumers(
        MassTransit.IRegistrationConfigurator registrationConfigurator
    ) =>
        ModuleRegistrationHelper.ConfigureConsumers(
            AssemblyReference.Assembly,
            registrationConfigurator
        );
}
