using Deep.Common.Application;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Deep.Common.Application.Outbox;
using Deep.Transactions.Application.Data;
using Deep.Transactions.Application.Features.PurchaseSaga;
using Deep.Transactions.Application.Inbox;
using Deep.Transactions.Application.Outbox;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Transactions.Application;

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
        ModuleRegistrationHelper.ConfigureConsumers(
            AssemblyReference.Assembly,
            registrationConfigurator,
            typeof(TransactionsIntegrationEventConsumer<>)
        );

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            registrationConfigurator
                .AddSagaStateMachine<PurchaseSaga, PurchaseSagaState>()
                .InMemoryRepository();
        }
        else
        {
            registrationConfigurator
                .AddSagaStateMachine<PurchaseSaga, PurchaseSagaState>()
                .RedisRepository(redisConnectionString);
        }
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
