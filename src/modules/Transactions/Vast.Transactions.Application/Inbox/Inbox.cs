using Vast.Common.Application.Dapper;
using Vast.Common.Application.Database;
using Vast.Common.Application.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vast.Transactions.Application.Inbox;

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
