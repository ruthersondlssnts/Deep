using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Deep.Common.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace Deep.Transactions.Application.Inbox;

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
