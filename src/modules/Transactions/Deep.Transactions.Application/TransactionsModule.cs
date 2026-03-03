using Deep.Common.Application;
using Deep.Common.Application.Database;
using Deep.Transactions.Application.BackgroundJobs;
using Deep.Transactions.Application.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Transactions.Application;

public static class TransactionsModule
{
    public const string ModuleName = "Transactions";

    public static IServiceCollection AddTransactionsModule(this IServiceCollection services)
    {
        services
            .AddValidation()
            .AddDomainEventHandlers(AssemblyReference.Assembly, Schemas.Transactions)
            .AddIntegrationEventHandlers(AssemblyReference.Assembly, Schemas.Transactions)
            .AddPostgresDbContextWithSchema<TransactionsDbContext>(Schemas.Transactions)
            .AddEndpoints(AssemblyReference.Assembly)
            .AddOutboxInterceptor<TransactionsDbContext>()
            .AddOutboxInboxJobs<TransactionsProcessOutboxJob, TransactionsProcessInboxJob, TransactionsInboxWriter>();
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
