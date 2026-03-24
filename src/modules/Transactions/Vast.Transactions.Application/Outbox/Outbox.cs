using Vast.Common.Application.Dapper;
using Vast.Common.Application.Database;
using Vast.Common.Application.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vast.Transactions.Application.Outbox;

public sealed class TransactionsOutboxProcessor(
    IDbConnectionFactory connectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<TransactionsOutboxProcessor> logger
)
    : OutboxProcessorBase(
        connectionFactory,
        serviceScopeFactory,
        options,
        logger,
        Schemas.Transactions,
        AssemblyReference.Assembly
    );

public sealed class TransactionsOutboxNotifier : OutboxNotifier
{
    public TransactionsOutboxNotifier() { }
}

public sealed class TransactionsInsertOutboxMessagesInterceptor(
    TransactionsOutboxNotifier outboxNotifier
) : InsertOutboxMessagesInterceptorBase(outboxNotifier);

public sealed class TransactionsOutboxBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    TransactionsOutboxNotifier notifier,
    IOptions<OutboxOptions> options,
    ILogger<TransactionsOutboxBackgroundService> logger
)
    : OutboxBackgroundService<TransactionsOutboxProcessor>(
        serviceScopeFactory,
        notifier,
        options,
        logger,
        Schemas.Transactions
    );
