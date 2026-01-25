using Deep.Accounts.Application.Data;
using Deep.Common.Application;
using Deep.Common.Application.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;

namespace Deep.Accounts.Application;

public static class AccountsModule
{
    public static IHostApplicationBuilder AddAccountsModule(this IHostApplicationBuilder builder)
    {
        return builder
            .AddDomainEventHandlers(AssemblyReference.Assembly)
            .AddPostgresDbContextWithSchema<AccountsDbContext>(builder.Configuration, Schemas.Accounts)
            .AddEndpoints(AssemblyReference.Assembly)
            .AddDomainEventInterceptor<AccountsDbContext>(AssemblyReference.Assembly);
    }

    public static void ConfigureConsumers(MassTransit.IRegistrationConfigurator registrationConfigurator) =>
        Deep.Common.Application.ModuleRegistrationHelper.ConfigureConsumers(AssemblyReference.Assembly, registrationConfigurator);
}
