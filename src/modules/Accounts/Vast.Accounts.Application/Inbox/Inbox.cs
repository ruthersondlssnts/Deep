using Vast.Common.Application.Dapper;
using Vast.Common.Application.Database;
using Vast.Common.Application.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vast.Accounts.Application.Inbox;

public sealed class AccountsInboxBackgroundService(
    IServiceScopeFactory scopeFactory,
    AccountsInboxNotifier notifier,
    IOptions<InboxOptions> options,
    ILogger<AccountsInboxBackgroundService> logger
)
    : InboxBackgroundService<AccountsInboxProcessor>(
        scopeFactory,
        notifier,
        options,
        logger,
        Schemas.Accounts
    );

public sealed class AccountsInboxNotifier : InboxNotifier
{
    public AccountsInboxNotifier() { }
}

public sealed class AccountsInboxProcessor(
    IDbConnectionFactory connectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<InboxOptions> options,
    ILogger<AccountsInboxProcessor> logger
)
    : InboxProcessorBase(
        connectionFactory,
        serviceScopeFactory,
        options,
        logger,
        Schemas.Accounts,
        AssemblyReference.Assembly
    );
