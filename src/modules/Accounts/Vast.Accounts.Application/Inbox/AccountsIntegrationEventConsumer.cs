using Vast.Common.Application.Dapper;
using Vast.Common.Application.Database;
using Vast.Common.Application.Inbox;
using Vast.Common.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace Vast.Accounts.Application.Inbox;

public sealed class AccountsIntegrationEventConsumer<TIntegrationEvent>(
    IDbConnectionFactory dbConnectionFactory,
    ILogger<AccountsIntegrationEventConsumer<TIntegrationEvent>> logger,
    AccountsInboxNotifier inboxNotifier
)
    : IntegrationEventConsumerBase<TIntegrationEvent>(
        dbConnectionFactory,
        logger,
        Schemas.Accounts,
        inboxNotifier
    )
    where TIntegrationEvent : class, IIntegrationEvent;
