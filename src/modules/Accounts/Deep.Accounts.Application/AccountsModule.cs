using Deep.Accounts.Application.Authentication;
using Deep.Accounts.Application.Authorization;
using Deep.Accounts.Application.Data;
using Deep.Accounts.Application.Inbox;
using Deep.Accounts.Application.Outbox;
using Deep.Accounts.Domain.Accounts;
using Deep.Common.Application;
using Deep.Common.Application.Authorization;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Deep.Common.Application.Outbox;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Accounts.Application;

public static class AccountsModule
{
    public const string ModuleName = "Accounts";

    public static IServiceCollection AddAccountsModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddValidation()
            .AddDomainEventHandlers(AssemblyReference.Assembly, Schemas.Accounts)
            .AddIntegrationEventHandlers(AssemblyReference.Assembly, Schemas.Accounts)
            .AddPostgresDbContext<AccountsDbContext, AccountsInsertOutboxMessagesInterceptor>(
                Schemas.Accounts,
                configuration
            )
            .AddEndpoints(AssemblyReference.Assembly)
            .AddAccountsOutbox()
            .AddAccountsInbox();

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

    public static void ConfigureConsumers(
        IRegistrationConfigurator registrationConfigurator,
        string? redisConnectionString = null
    ) =>
        ModuleRegistrationHelper.ConfigureConsumers(
            AssemblyReference.Assembly,
            registrationConfigurator,
            typeof(AccountsIntegrationEventConsumer<>)
        );

    public static IServiceCollection AddAccountsOutbox(this IServiceCollection services)
    {
        services.AddSingleton<AccountsOutboxNotifier>();
        services.AddScoped<AccountsOutboxProcessor>();
        services.AddScoped<AccountsInsertOutboxMessagesInterceptor>();
        services.AddHostedService<AccountsOutboxBackgroundService>();

        return services;
    }

    public static IServiceCollection AddAccountsInbox(this IServiceCollection services)
    {
        services.AddSingleton<AccountsInboxNotifier>();
        services.AddScoped<AccountsInboxProcessor>();
        services.AddHostedService<AccountsInboxBackgroundService>();

        return services;
    }
}
