using Vast.Common.Application.Dapper;
using Vast.Common.Application.Database;
using Vast.Common.Application.Inbox;
using Vast.Common.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace Vast.Programs.Application.Inbox;

public sealed class ProgramsIntegrationEventConsumer<TIntegrationEvent>(
    IDbConnectionFactory dbConnectionFactory,
    ILogger<ProgramsIntegrationEventConsumer<TIntegrationEvent>> logger,
    ProgramsInboxNotifier inboxNotifier
)
    : IntegrationEventConsumerBase<TIntegrationEvent>(
        dbConnectionFactory,
        logger,
        Schemas.Programs,
        inboxNotifier
    )
    where TIntegrationEvent : class, IIntegrationEvent;
