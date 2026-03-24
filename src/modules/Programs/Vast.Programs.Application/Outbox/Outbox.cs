using Vast.Common.Application.Dapper;
using Vast.Common.Application.Database;
using Vast.Common.Application.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vast.Programs.Application.Outbox;

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
