using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deep.Programs.Application.BackgroundJobs;

public sealed class ProgramsProcessInboxJob(
    IDbConnectionFactory connectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<InboxOptions> options,
    ILogger<ProgramsProcessInboxJob> logger
) : ProcessInboxJobBase(
    connectionFactory,
    serviceScopeFactory,
    options,
    logger,
    Schemas.Programs,
    AssemblyReference.Assembly
);
