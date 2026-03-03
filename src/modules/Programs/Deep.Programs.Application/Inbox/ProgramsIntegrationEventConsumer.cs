using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Deep.Common.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace Deep.Programs.Application.Inbox;

public sealed class ProgramsIntegrationEventConsumer<TIntegrationEvent>(
    IDbConnectionFactory dbConnectionFactory,
    ILogger<ProgramsIntegrationEventConsumer<TIntegrationEvent>> logger
) : IntegrationEventConsumerBase<TIntegrationEvent>(dbConnectionFactory, logger, Schemas.Programs)
    where TIntegrationEvent : class, IIntegrationEvent;
