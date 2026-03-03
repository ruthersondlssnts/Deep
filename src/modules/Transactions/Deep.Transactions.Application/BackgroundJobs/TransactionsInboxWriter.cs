using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Microsoft.Extensions.Logging;

namespace Deep.Transactions.Application.BackgroundJobs;

public sealed class TransactionsInboxWriter(
    IDbConnectionFactory connectionFactory,
    ILogger<TransactionsInboxWriter> logger
) : InboxWriterBase(connectionFactory, logger, Schemas.Transactions);
