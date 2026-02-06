using Deep.Accounts.Application.Data;
using Deep.Common.Application;
using Deep.Common.Application.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Accounts.Application;

public static class AccountsModule
{
    public static IServiceCollection AddAccountsModule(this IServiceCollection services)
    {
        services.AddDomainEventHandlers(AssemblyReference.Assembly)
                .AddPostgresDbContextWithSchema<AccountsDbContext>(Schemas.Accounts)
                .AddEndpoints(AssemblyReference.Assembly)
                .AddDomainEventInterceptor<AccountsDbContext>(AssemblyReference.Assembly);
        return services;
    }

    public static void ConfigureConsumers(MassTransit.IRegistrationConfigurator registrationConfigurator) =>
        Deep.Common.Application.ModuleRegistrationHelper.ConfigureConsumers(AssemblyReference.Assembly, registrationConfigurator);
}
