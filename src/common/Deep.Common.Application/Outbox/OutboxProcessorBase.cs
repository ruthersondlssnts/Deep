using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text.Json;
using Dapper;
using Deep.Common.Application.Dapper;
using Deep.Common.Application.DomainEvents;
using Deep.Common.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deep.Common.Application.Outbox;

public abstract partial class OutboxProcessorBase(
    IDbConnectionFactory connectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<OutboxOptions> options,
    ILogger logger,
    string schema,
    Assembly applicationAssembly
) : IOutboxProcessor
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger _logger = logger;
    private readonly OutboxOptions _options = options.Value;
    private readonly string _schema = schema;
    private readonly Assembly _applicationAssembly = applicationAssembly;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<int> ProcessAsync(CancellationToken cancellationToken = default)
    {
        LogStartingOutboxProcessing(_logger, _schema);

        await using DbConnection connection = await _connectionFactory.OpenConnectionAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken
        );

        try
        {
            IReadOnlyList<OutboxMessageData> messages = await FetchMessagesAsync(
                connection,
                transaction
            );

            if (messages.Count == 0)
            {
                LogNoOutboxMessagesToProcess(_logger, _schema);
                await transaction.CommitAsync(cancellationToken);
                return 0;
            }

            LogProcessingOutboxMessages(_logger, messages.Count, _schema);

            List<ProcessedOutboxMessage> processedMessages = [];

            foreach (OutboxMessageData message in messages)
            {
                string? error = await ProcessMessageAsync(message, cancellationToken);
                processedMessages.Add(new ProcessedOutboxMessage(message.Id, error));
            }

            await UpdateMessagesAsync(
                processedMessages,
                connection,
                transaction,
                cancellationToken
            );

            await transaction.CommitAsync(cancellationToken);

            LogCompletedOutboxProcessing(_logger, _schema, messages.Count);

            return messages.Count;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<IReadOnlyList<OutboxMessageData>> FetchMessagesAsync(
        DbConnection connection,
        DbTransaction transaction
    )
    {
        string sql = $"""
            SELECT id AS Id, type AS Type, content AS Content
            FROM {_schema}.outbox_messages
            WHERE processed_at_utc IS NULL
            ORDER BY occurred_at_utc
            LIMIT @BatchSize
            FOR UPDATE SKIP LOCKED;
            """;

        IEnumerable<OutboxMessageData> messages = await connection.QueryAsync<OutboxMessageData>(
            sql,
            new { _options.BatchSize },
            transaction
        );

        return messages.ToList();
    }

    private async Task<string?> ProcessMessageAsync(
        OutboxMessageData message,
        CancellationToken cancellationToken
    )
    {
        try
        {
            Type? domainEventType = DomainEventHandlersFactory.GetDomainEventType(
                message.Type,
                _applicationAssembly
            );

            domainEventType ??= Type.GetType(message.Type);

            if (domainEventType is null)
            {
                LogDomainEventTypeNotFound(_logger, message.Type);
                return $"Domain event type not found: {message.Type}";
            }

            object? deserialized = JsonSerializer.Deserialize(
                message.Content,
                domainEventType,
                JsonSerializerOptions
            );

            if (deserialized is not IDomainEvent domainEvent)
            {
                LogFailedToDeserializeDomainEvent(_logger, message.Type);
                return $"Failed to deserialize domain event: {message.Type}";
            }

            await ExecuteHandlersAsync(domainEvent, domainEventType, cancellationToken);

            return null;
        }
        catch (Exception ex)
        {
            LogErrorProcessingOutboxMessage(_logger, ex, message.Id);
            return ex.ToString();
        }
    }

    private async Task ExecuteHandlersAsync(
        IDomainEvent domainEvent,
        Type domainEventType,
        CancellationToken cancellationToken
    )
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();

        var handlers = DomainEventHandlersFactory
            .GetHandlers(domainEventType, scope.ServiceProvider, _applicationAssembly)
            .ToList();

        if (handlers.Count == 0)
        {
            return;
        }

        Task[] tasks = handlers
            .Select(handler => handler.Handle(domainEvent, cancellationToken))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    private async Task UpdateMessagesAsync(
        IReadOnlyCollection<ProcessedOutboxMessage> messages,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken
    )
    {
        DateTime processedAtUtc = DateTime.UtcNow;

        string sql = $"""
            UPDATE {_schema}.outbox_messages
            SET processed_at_utc = @ProcessedAtUtc,
                error = @Error
            WHERE id = @Id;
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                messages.Select(message => new
                {
                    message.Id,
                    ProcessedAtUtc = processedAtUtc,
                    message.Error,
                }),
                transaction,
                cancellationToken: cancellationToken
            )
        );
    }

    private sealed record OutboxMessageData(Guid Id, string Type, string Content);

    private sealed record ProcessedOutboxMessage(Guid Id, string? Error);

    [LoggerMessage(
        EventId = 3000,
        Level = LogLevel.Debug,
        Message = "Starting outbox processing for schema {Schema}"
    )]
    private static partial void LogStartingOutboxProcessing(ILogger logger, string schema);

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Debug,
        Message = "No outbox messages to process for schema {Schema}"
    )]
    private static partial void LogNoOutboxMessagesToProcess(ILogger logger, string schema);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Information,
        Message = "Processing {Count} outbox messages for schema {Schema}"
    )]
    private static partial void LogProcessingOutboxMessages(
        ILogger logger,
        int count,
        string schema
    );

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Information,
        Message = "Completed outbox processing for schema {Schema}. Processed {Count} messages"
    )]
    private static partial void LogCompletedOutboxProcessing(
        ILogger logger,
        string schema,
        int count
    );

    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Warning,
        Message = "Domain event type not found: {Type}"
    )]
    private static partial void LogDomainEventTypeNotFound(ILogger logger, string type);

    [LoggerMessage(
        EventId = 3005,
        Level = LogLevel.Warning,
        Message = "Failed to deserialize domain event: {Type}"
    )]
    private static partial void LogFailedToDeserializeDomainEvent(ILogger logger, string type);

    [LoggerMessage(
        EventId = 3006,
        Level = LogLevel.Error,
        Message = "Error processing outbox message {MessageId}"
    )]
    private static partial void LogErrorProcessingOutboxMessage(
        ILogger logger,
        Exception exception,
        Guid messageId
    );
}
