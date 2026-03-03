using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Microsoft.Extensions.Logging;

namespace Deep.Accounts.Application.BackgroundJobs;

public sealed class AccountsInboxWriter(
    IDbConnectionFactory connectionFactory,
    ILogger<AccountsInboxWriter> logger
) : InboxWriterBase(connectionFactory, logger, Schemas.Accounts);
