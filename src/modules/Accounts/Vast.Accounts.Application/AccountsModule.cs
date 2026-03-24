using Vast.Accounts.Application.Authentication;
using Vast.Accounts.Application.Authorization;
using Vast.Accounts.Application.Data;
using Vast.Accounts.Application.Inbox;
using Vast.Accounts.Application.Outbox;
using Vast.Accounts.Domain.Accounts;
using Vast.Common.Application;
using Vast.Common.Application.Authorization;
using Vast.Common.Application.Database;
using Vast.Common.Application.Inbox;
using Vast.Common.Application.Outbox;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Vast.Accounts.Application;

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
        string? _ = null
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
