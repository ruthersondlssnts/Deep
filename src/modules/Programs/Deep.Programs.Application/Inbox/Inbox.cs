using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deep.Programs.Application.Inbox;

public sealed class ProgramsInboxBackgroundService(
    IServiceScopeFactory scopeFactory,
    ProgramsInboxNotifier notifier,
    IOptions<InboxOptions> options,
    ILogger<ProgramsInboxBackgroundService> logger
)
    : InboxBackgroundService<ProgramsInboxProcessor>(
        scopeFactory,
        notifier,
        options,
        logger,
        Schemas.Programs
    );

public sealed class ProgramsInboxNotifier : InboxNotifier
{
    public ProgramsInboxNotifier() { }
}

public sealed class ProgramsInboxProcessor(
    IDbConnectionFactory connectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<InboxOptions> options,
    ILogger<ProgramsInboxProcessor> logger
)
    : InboxProcessorBase(
        connectionFactory,
        serviceScopeFactory,
        options,
        logger,
        Schemas.Programs,
        AssemblyReference.Assembly
    );
