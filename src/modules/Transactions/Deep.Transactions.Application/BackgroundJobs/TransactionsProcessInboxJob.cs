using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deep.Transactions.Application.BackgroundJobs;

public sealed class TransactionsProcessInboxJob(
    IDbConnectionFactory connectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<InboxOptions> options,
    ILogger<TransactionsProcessInboxJob> logger
)
    : ProcessInboxJobBase(
        connectionFactory,
        serviceScopeFactory,
        options,
        logger,
        Schemas.Transactions,
        AssemblyReference.Assembly
    );
