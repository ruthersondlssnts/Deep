using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Deep.Common.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace Deep.Accounts.Application.Inbox;

public sealed class AccountsIntegrationEventConsumer<TIntegrationEvent>(
    IDbConnectionFactory dbConnectionFactory,
    ILogger<AccountsIntegrationEventConsumer<TIntegrationEvent>> logger
) : IntegrationEventConsumerBase<TIntegrationEvent>(dbConnectionFactory, logger, Schemas.Accounts)
    where TIntegrationEvent : class, IIntegrationEvent;
