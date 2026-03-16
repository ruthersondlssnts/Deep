using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deep.Transactions.Application.Outbox;

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

public sealed class TransactionsOutboxNotifier : OutboxNotifier;

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
