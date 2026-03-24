using Vast.Common.Application.Dapper;
using Vast.Common.Application.Database;
using Vast.Common.Application.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vast.Accounts.Application.Outbox;

public sealed class AccountsOutboxProcessor(
    IDbConnectionFactory connectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<AccountsOutboxProcessor> logger
)
    : OutboxProcessorBase(
        connectionFactory,
        serviceScopeFactory,
        options,
        logger,
        Schemas.Accounts,
        AssemblyReference.Assembly
    );

public sealed class AccountsOutboxNotifier : OutboxNotifier
{
    public AccountsOutboxNotifier() { }
}

public sealed class AccountsInsertOutboxMessagesInterceptor(AccountsOutboxNotifier outboxNotifier)
    : InsertOutboxMessagesInterceptorBase(outboxNotifier);

public sealed class AccountsOutboxBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    AccountsOutboxNotifier notifier,
    IOptions<OutboxOptions> options,
    ILogger<AccountsOutboxBackgroundService> logger
)
    : OutboxBackgroundService<AccountsOutboxProcessor>(
        serviceScopeFactory,
        notifier,
        options,
        logger,
        Schemas.Accounts
    );
