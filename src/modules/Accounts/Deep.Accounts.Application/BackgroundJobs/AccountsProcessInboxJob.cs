using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deep.Accounts.Application.BackgroundJobs;

public sealed class AccountsProcessInboxJob(
    IDbConnectionFactory connectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<InboxOptions> options,
    ILogger<AccountsProcessInboxJob> logger
) : ProcessInboxJobBase(
    connectionFactory,
    serviceScopeFactory,
    options,
    logger,
    Schemas.Accounts,
    AssemblyReference.Assembly
);
