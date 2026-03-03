using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Deep.Transactions.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace Deep.Programs.Application.InboxConsumers;

public sealed class TransactionCreatedIntegrationEventConsumer(
    IDbConnectionFactory dbConnectionFactory,
    ILogger<TransactionCreatedIntegrationEventConsumer> logger
) : IntegrationEventConsumerBase<TransactionCreatedIntegrationEvent>(
    dbConnectionFactory,
    logger,
    Schemas.Programs)
{
}
