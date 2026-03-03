using Deep.Accounts.IntegrationEvents;
using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Microsoft.Extensions.Logging;

namespace Deep.Programs.Application.InboxConsumers;

public sealed class AccountRegisteredIntegrationEventConsumer(
    IDbConnectionFactory dbConnectionFactory,
    ILogger<AccountRegisteredIntegrationEventConsumer> logger
) : IntegrationEventConsumerBase<AccountRegisteredIntegrationEvent>(
    dbConnectionFactory,
    logger,
    Schemas.Programs)
{
}
