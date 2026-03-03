using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deep.Transactions.Application.BackgroundJobs;

public sealed class TransactionsProcessOutboxJob(
    IDbConnectionFactory connectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<TransactionsProcessOutboxJob> logger
)
    : ProcessOutboxJobBase(
        connectionFactory,
        serviceScopeFactory,
        options,
        logger,
        Schemas.Transactions,
        AssemblyReference.Assembly
    );
