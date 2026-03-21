using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deep.Transactions.Application.Inbox;

public sealed class TransactionsInboxBackgroundService(
    IServiceScopeFactory scopeFactory,
    TransactionsInboxNotifier notifier,
    IOptions<InboxOptions> options,
    ILogger<TransactionsInboxBackgroundService> logger
)
    : InboxBackgroundService<TransactionsInboxProcessor>(
        scopeFactory,
        notifier,
        options,
        logger,
        Schemas.Transactions
    );

public sealed class TransactionsInboxNotifier : InboxNotifier
{
    public TransactionsInboxNotifier() { }
}

public sealed class TransactionsInboxProcessor(
    IDbConnectionFactory connectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<InboxOptions> options,
    ILogger<TransactionsInboxProcessor> logger
)
    : InboxProcessorBase(
        connectionFactory,
        serviceScopeFactory,
        options,
        logger,
        Schemas.Transactions,
        AssemblyReference.Assembly
    );
