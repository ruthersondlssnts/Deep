using Deep.Common.Application;
using Deep.Common.Application.Database;
using Deep.Transactions.Application.Data;
using Deep.Transactions.Domain.Customer;
using Deep.Transactions.Domain.Transaction;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Transactions.Application;

public static class TransactionsModule
{
    public static IServiceCollection AddTransactionsModule(this IServiceCollection services)
    {
        services
            .AddDomainEventHandlers(AssemblyReference.Assembly)
            .AddPostgresDbContextWithSchema<TransactionsDbContext>(Schemas.Transactions)
            .AddEndpoints(AssemblyReference.Assembly)
            .AddDomainEventInterceptor<TransactionsDbContext>(AssemblyReference.Assembly)
            .AddScoped<ITransactionRepository, TransactionRepository>()
            .AddScoped<ICustomerRepository, CustomerRepository>();
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
