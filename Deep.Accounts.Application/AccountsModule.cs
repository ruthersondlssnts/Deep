using Deep.Accounts.Application.Authentication;
using Deep.Accounts.Application.Authorization;
using Deep.Accounts.Application.Data;
using Deep.Accounts.Domain.Accounts;
using Deep.Common.Application;
using Deep.Common.Application.Authorization;
using Deep.Common.Application.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Accounts.Application;

public static class AccountsModule
{
    public static IServiceCollection AddAccountsModule(this IServiceCollection services)
    {
        services
            .AddDomainEventHandlers(AssemblyReference.Assembly)
            .AddPostgresDbContextWithSchema<AccountsDbContext>(Schemas.Accounts)
            .AddEndpoints(AssemblyReference.Assembly)
            .AddDomainEventInterceptor<AccountsDbContext>(AssemblyReference.Assembly);

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
        MassTransit.IRegistrationConfigurator registrationConfigurator
    ) =>
        Deep.Common.Application.ModuleRegistrationHelper.ConfigureConsumers(
            AssemblyReference.Assembly,
            registrationConfigurator
        );
}
