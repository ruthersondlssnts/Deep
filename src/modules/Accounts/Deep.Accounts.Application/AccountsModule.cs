using Deep.Accounts.Application.Authentication;
using Deep.Accounts.Application.Authorization;
using Deep.Accounts.Application.BackgroundJobs;
using Deep.Accounts.Application.Data;
using Deep.Accounts.Domain.Accounts;
using Deep.Common.Application;
using Deep.Common.Application.Authorization;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Deep.Common.Application.Outbox;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Accounts.Application;

public static class AccountsModule
{
    public const string ModuleName = "Accounts";

    public static IServiceCollection AddAccountsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddValidation()
            .AddDomainEventHandlers(AssemblyReference.Assembly, Schemas.Accounts)
            .AddIntegrationEventHandlers(AssemblyReference.Assembly, Schemas.Accounts)
            .AddPostgresDbContext<AccountsDbContext>(Schemas.Accounts, configuration)
            .AddEndpoints(AssemblyReference.Assembly)
            .AddOutboxInboxJobs<AccountsProcessOutboxJob, AccountsProcessInboxJob, AccountsInboxWriter>();

        services.Configure<OutboxOptions>(configuration.GetSection("Accounts:Outbox"));
        services.Configure<InboxOptions>(configuration.GetSection("Accounts:Inbox"));

        services
            .AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.SectionName)
            .ValidateDataAnnotations();

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();
        services.AddScoped<IPermissionService, PermissionService>();

        return services;
    }

    public static void ConfigureConsumers(MassTransit.IRegistrationConfigurator registrationConfigurator) =>
        ModuleRegistrationHelper.ConfigureConsumers(AssemblyReference.Assembly, registrationConfigurator);
}
