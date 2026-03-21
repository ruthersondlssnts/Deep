using System.Data.Common;
using Dapper;
using Deep.Common.Application.Dapper;
using Deep.Common.Application.DomainEvents;
using Deep.Common.Domain;
using Microsoft.Extensions.Logging;

namespace Deep.Common.Application.Outbox;

public sealed partial class IdempotentDomainEventHandler<TDomainEvent>(
    IDomainEventHandler<TDomainEvent> decorated,
    IDbConnectionFactory dbConnectionFactory,
    ILogger<IdempotentDomainEventHandler<TDomainEvent>> logger,
    string schema
) : DomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    public override async Task Handle(
        TDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        string consumerName = decorated.GetType().Name;

        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync(
            cancellationToken
        );

        int affectedRows = await InsertOutboxConsumerAsync(
            connection,
            transaction,
            schema,
            domainEvent.Id,
            consumerName
        );

        if (affectedRows == 0)
        {
            LogAlreadyProcessed(logger, domainEvent.Id, consumerName);

            await transaction.RollbackAsync(cancellationToken);
            return;
        }

        LogProcessing(logger, domainEvent.Id, consumerName);

        try
        {
            await decorated.Handle(domainEvent, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task<int> InsertOutboxConsumerAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        Guid outboxMessageId,
        string name
    )
    {
        string sql = $"""
            INSERT INTO {schema}.outbox_message_consumers (outbox_message_id, name)
            VALUES (@OutboxMessageId, @Name)
            ON CONFLICT DO NOTHING;
            """;

        return await connection.ExecuteAsync(
            sql,
            new { OutboxMessageId = outboxMessageId, Name = name },
            transaction
        );
    }

    [LoggerMessage(
        EventId = 5000,
        Level = LogLevel.Debug,
        Message = "Domain event {EventId} already processed by handler {Handler}"
    )]
    private static partial void LogAlreadyProcessed(ILogger logger, Guid eventId, string handler);

    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Debug,
        Message = "Processing domain event {EventId} with handler {Handler}"
    )]
    private static partial void LogProcessing(ILogger logger, Guid eventId, string handler);
}
