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

public abstract class ProcessOutboxJobBase(
    IDbConnectionFactory connectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<OutboxOptions> options,
    ILogger logger,
    string schema,
    Assembly applicationAssembly
)
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger _logger = logger;
    private readonly OutboxOptions _options = options.Value;
    private readonly string _schema = schema;
    private readonly Assembly _applicationAssembly = applicationAssembly;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting outbox processing for schema {Schema}", _schema);

        await using DbConnection connection = await _connectionFactory.OpenConnectionAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            IReadOnlyList<OutboxMessageData> messages = await FetchMessagesAsync(connection, transaction);

            if (messages.Count == 0)
            {
                _logger.LogDebug("No outbox messages to process for schema {Schema}", _schema);
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            _logger.LogInformation("Processing {Count} outbox messages for schema {Schema}", messages.Count, _schema);

            foreach (OutboxMessageData message in messages)
            {
                await ProcessMessageAsync(message, connection, transaction, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Completed outbox processing for schema {Schema}", _schema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during outbox processing for schema {Schema}", _schema);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<IReadOnlyList<OutboxMessageData>> FetchMessagesAsync(
        DbConnection connection,
        DbTransaction transaction)
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
            transaction);

        return messages.ToList();
    }

    private async Task ProcessMessageAsync(
        OutboxMessageData message,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken)
    {
        string? error = null;

        try
        {
            Type? domainEventType = DomainEventHandlersFactory.GetDomainEventType(
                message.Type,
                _applicationAssembly
            );

            domainEventType ??= Type.GetType(message.Type);

            if (domainEventType is null)
            {
                error = $"Domain event type not found: {message.Type}";
                _logger.LogWarning("Domain event type not found: {Type}", message.Type);
            }
            else if (JsonSerializer.Deserialize(message.Content, domainEventType, JsonSerializerOptions) is IDomainEvent domainEvent)
            {
                await ExecuteHandlersAsync(domainEvent, domainEventType, cancellationToken);
            }
            else
            {
                error = $"Failed to deserialize domain event: {message.Type}";
                _logger.LogWarning("Failed to deserialize domain event: {Type}", message.Type);
            }
        }
        catch (Exception ex)
        {
            error = ex.Message;
            _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
        }

        await UpdateMessageAsync(message.Id, error, connection, transaction);
    }

    private async Task ExecuteHandlersAsync(
        IDomainEvent domainEvent,
        Type domainEventType,
        CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();

        IEnumerable<IDomainEventHandler> handlers = DomainEventHandlersFactory.GetHandlers(
            domainEventType,
            scope.ServiceProvider,
            _applicationAssembly);

        foreach (IDomainEventHandler handler in handlers)
        {
            await handler.Handle(domainEvent, cancellationToken);
        }
    }

    private async Task UpdateMessageAsync(
        Guid messageId,
        string? error,
        DbConnection connection,
        DbTransaction transaction)
    {
        string sql = $"""
            UPDATE {_schema}.outbox_messages
            SET processed_at_utc = @ProcessedAtUtc, error = @Error
            WHERE id = @Id;
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                Id = messageId,
                ProcessedAtUtc = DateTime.UtcNow,
                Error = error
            },
            transaction);
    }

    private sealed record OutboxMessageData(Guid Id, string Type, string Content);
}
