using System.Data.Common;
using Dapper;
using Vast.Common.Application.Dapper;
using Vast.Common.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace Vast.Common.Application.Inbox;

public sealed partial class IdempotentIntegrationEventHandler<TIntegrationEvent>(
    IIntegrationEventHandler<TIntegrationEvent> decorated,
    IDbConnectionFactory dbConnectionFactory,
    ILogger<IdempotentIntegrationEventHandler<TIntegrationEvent>> logger,
    string schema
) : IntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    public override async Task Handle(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        string consumerName = decorated.GetType().Name;

        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync(
            cancellationToken
        );

        int affectedRows = await InsertInboxConsumerAsync(
            connection,
            transaction,
            schema,
            integrationEvent.Id,
            consumerName
        );

        if (affectedRows == 0)
        {
            LogAlreadyProcessed(logger, integrationEvent.Id, consumerName);
            return;
        }

        LogProcessing(logger, integrationEvent.Id, consumerName);

        try
        {
            await decorated.Handle(integrationEvent, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task<int> InsertInboxConsumerAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        Guid inboxMessageId,
        string name
    )
    {
        string sql = $"""
            INSERT INTO {schema}.inbox_message_consumers (inbox_message_id, name)
            VALUES (@InboxMessageId, @Name)
            ON CONFLICT DO NOTHING;
            """;

        return await connection.ExecuteAsync(
            sql,
            new { InboxMessageId = inboxMessageId, Name = name },
            transaction
        );
    }

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Integration event {EventId} already processed by handler {Handler}"
    )]
    private static partial void LogAlreadyProcessed(ILogger logger, Guid eventId, string handler);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Processing integration event {EventId} with handler {Handler}"
    )]
    private static partial void LogProcessing(ILogger logger, Guid eventId, string handler);
}
