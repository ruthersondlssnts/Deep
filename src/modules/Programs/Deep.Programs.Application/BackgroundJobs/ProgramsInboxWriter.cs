using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Microsoft.Extensions.Logging;

namespace Deep.Programs.Application.BackgroundJobs;

public sealed class ProgramsInboxWriter(
    IDbConnectionFactory connectionFactory,
    ILogger<ProgramsInboxWriter> logger
) : InboxWriterBase(connectionFactory, logger, Schemas.Programs);
