using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deep.Programs.Application.Outbox;

public sealed class ProgramsOutboxProcessor(
    IDbConnectionFactory connectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<ProgramsOutboxProcessor> logger
)
    : OutboxProcessorBase(
        connectionFactory,
        serviceScopeFactory,
        options,
        logger,
        Schemas.Programs,
        AssemblyReference.Assembly
    );

public sealed class ProgramsOutboxNotifier : OutboxNotifier
{
    public ProgramsOutboxNotifier() { }
}

public sealed class ProgramsInsertOutboxMessagesInterceptor(ProgramsOutboxNotifier outboxNotifier)
    : InsertOutboxMessagesInterceptorBase(outboxNotifier);

public sealed class ProgramsOutboxBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    ProgramsOutboxNotifier notifier,
    IOptions<OutboxOptions> options,
    ILogger<ProgramsOutboxBackgroundService> logger
)
    : OutboxBackgroundService<ProgramsOutboxProcessor>(
        serviceScopeFactory,
        notifier,
        options,
        logger,
        Schemas.Programs
    );
