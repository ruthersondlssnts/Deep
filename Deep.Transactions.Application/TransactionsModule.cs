using Deep.Transactions.Application.Data;
using Deep.Common.Application;
using Deep.Common.Application.Database;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;

namespace Deep.Transactions.Application;
public static class TransactionsModule
{
    public static IHostApplicationBuilder AddTransactionsModule(this IHostApplicationBuilder builder)
    {
        return builder
            .AddDomainEventHandlers(AssemblyReference.Assembly)
            .AddPostgresDbContextWithSchema<TransactionsDbContext>(builder.Configuration, Schemas.Transactions)
            .AddEndpoints(AssemblyReference.Assembly)
            .AddDomainEventInterceptor<TransactionsDbContext>(AssemblyReference.Assembly);
    }

    public static void ConfigureConsumers(MassTransit.IRegistrationConfigurator registrationConfigurator) =>
        ModuleRegistrationHelper.ConfigureConsumers(AssemblyReference.Assembly, registrationConfigurator);
}
