using Vast.Common.Application.Dapper;
using Vast.Common.Application.Database;
using Vast.Common.Application.Inbox;
using Vast.Common.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace Vast.Transactions.Application.Inbox;

public sealed class TransactionsIntegrationEventConsumer<TIntegrationEvent>(
    IDbConnectionFactory dbConnectionFactory,
    ILogger<TransactionsIntegrationEventConsumer<TIntegrationEvent>> logger,
    TransactionsInboxNotifier inboxNotifier
)
    : IntegrationEventConsumerBase<TIntegrationEvent>(
        dbConnectionFactory,
        logger,
        Schemas.Transactions,
        inboxNotifier
    )
    where TIntegrationEvent : class, IIntegrationEvent;
