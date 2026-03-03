using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deep.Accounts.Application.BackgroundJobs;

public sealed class AccountsProcessOutboxJob(
    IDbConnectionFactory connectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<AccountsProcessOutboxJob> logger
)
    : ProcessOutboxJobBase(
        connectionFactory,
        serviceScopeFactory,
        options,
        logger,
        Schemas.Accounts,
        AssemblyReference.Assembly
    );
