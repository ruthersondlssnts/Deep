using Vast.Common.Application;
using Vast.Common.Application.Database;
using Vast.Common.Application.Inbox;
using Vast.Common.Application.Outbox;
using Vast.Transactions.Application.Data;
using Vast.Transactions.Application.Features.PurchaseSaga;
using Vast.Transactions.Application.Inbox;
using Vast.Transactions.Application.Outbox;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Vast.Transactions.Application;

public static class TransactionsModule
{
    public const string ModuleName = "Transactions";

    public static IServiceCollection AddTransactionsModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddValidation()
            .AddDomainEventHandlers(AssemblyReference.Assembly, Schemas.Transactions)
            .AddIntegrationEventHandlers(AssemblyReference.Assembly, Schemas.Transactions)
            .AddPostgresDbContext<
                TransactionsDbContext,
                TransactionsInsertOutboxMessagesInterceptor
            >(Schemas.Transactions, configuration)
            .AddEndpoints(AssemblyReference.Assembly)
            .AddTransactionsInbox()
            .AddTransactionsOutbox();

        services.Configure<OutboxOptions>(configuration.GetSection("Transactions:Outbox"));
        services.Configure<InboxOptions>(configuration.GetSection("Transactions:Inbox"));

        return services;
    }

    public static void ConfigureConsumers(
        IRegistrationConfigurator registrationConfigurator,
        string? redisConnectionString = null
    )
    {
        registrationConfigurator
            .AddSagaStateMachine<PurchaseSaga, PurchaseSagaState>()
            .RedisRepository(redisConnectionString);

        ModuleRegistrationHelper.ConfigureConsumers(
            AssemblyReference.Assembly,
            registrationConfigurator,
            typeof(TransactionsIntegrationEventConsumer<>)
        );
    }

    public static IServiceCollection AddTransactionsOutbox(this IServiceCollection services)
    {
        services.AddSingleton<TransactionsOutboxNotifier>();
        services.AddScoped<TransactionsOutboxProcessor>();
        services.AddScoped<TransactionsInsertOutboxMessagesInterceptor>();
        services.AddHostedService<TransactionsOutboxBackgroundService>();

        return services;
    }

    public static IServiceCollection AddTransactionsInbox(this IServiceCollection services)
    {
        services.AddSingleton<TransactionsInboxNotifier>();
        services.AddScoped<TransactionsInboxProcessor>();
        services.AddHostedService<TransactionsInboxBackgroundService>();

        return services;
    }
}
