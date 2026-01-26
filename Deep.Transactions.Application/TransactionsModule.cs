using Deep.Transactions.Application.Data;
using Deep.Common.Application.Database;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using Deep.Common.Application;

namespace Deep.Transactions.Application;
public static class TransactionsModule
{
    public static IServiceCollection AddTransactionsModule(this IServiceCollection services)
    {
        services.AddDomainEventHandlers(AssemblyReference.Assembly)
                .AddPostgresDbContextWithSchema<TransactionsDbContext>( Schemas.Transactions)
                .AddEndpoints(AssemblyReference.Assembly)
                .AddDomainEventInterceptor<TransactionsDbContext>(AssemblyReference.Assembly);
        return services;
    }

    public static void ConfigureConsumers(MassTransit.IRegistrationConfigurator registrationConfigurator) =>
        ModuleRegistrationHelper.ConfigureConsumers(AssemblyReference.Assembly, registrationConfigurator);
}
