using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text.Json;
using Dapper;
using Deep.Common.Application.Dapper;
using Deep.Common.Application.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deep.Common.Application.Inbox;

public abstract class ProcessInboxJobBase(
    IDbConnectionFactory connectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<InboxOptions> options,
    ILogger logger,
    string schema,
    Assembly presentationAssembly
)
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger _logger = logger;
    private readonly InboxOptions _options = options.Value;
    private readonly string _schema = schema;
    private readonly Assembly _presentationAssembly = presentationAssembly;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting inbox processing for schema {Schema}", _schema);

        await using DbConnection connection = await _connectionFactory.OpenConnectionAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken
        );

        try
        {
            IReadOnlyList<InboxMessageData> messages = await FetchMessagesAsync(
                connection,
                transaction
            );

            if (messages.Count == 0)
            {
                _logger.LogDebug("No inbox messages to process for schema {Schema}", _schema);
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            _logger.LogInformation(
                "Processing {Count} inbox messages for schema {Schema}",
                messages.Count,
                _schema
            );

            foreach (InboxMessageData message in messages)
            {
                await ProcessMessageAsync(message, connection, transaction, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Completed inbox processing for schema {Schema}", _schema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during inbox processing for schema {Schema}", _schema);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<IReadOnlyList<InboxMessageData>> FetchMessagesAsync(
        DbConnection connection,
        DbTransaction transaction
    )
    {
        string sql = $"""
            SELECT id AS Id, type AS Type, content AS Content
            FROM {_schema}.inbox_messages
            WHERE processed_at_utc IS NULL
            ORDER BY occurred_at_utc
            LIMIT @BatchSize
            FOR UPDATE SKIP LOCKED;
            """;

        IEnumerable<InboxMessageData> messages = await connection.QueryAsync<InboxMessageData>(
            sql,
            new { _options.BatchSize },
            transaction
        );

        return messages.ToList();
    }

    private async Task ProcessMessageAsync(
        InboxMessageData message,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken
    )
    {
        string? error = null;

        try
        {
            Type? integrationEventType = IntegrationEventHandlersFactory.GetIntegrationEventType(
                message.Type,
                _presentationAssembly
            );

            integrationEventType ??= Type.GetType(message.Type);

            if (integrationEventType is null)
            {
                error = $"Integration event type not found: {message.Type}";
                _logger.LogWarning("Integration event type not found: {Type}", message.Type);
            }
            else if (
                JsonSerializer.Deserialize(
                    message.Content,
                    integrationEventType,
                    JsonSerializerOptions
                )
                is IIntegrationEvent integrationEvent
            )
            {
                await ExecuteHandlersAsync(
                    integrationEvent,
                    integrationEventType,
                    cancellationToken
                );
            }
            else
            {
                error = $"Failed to deserialize integration event: {message.Type}";
                _logger.LogWarning("Failed to deserialize integration event: {Type}", message.Type);
            }
        }
        catch (Exception ex)
        {
            error = ex.Message;
            _logger.LogError(ex, "Error processing inbox message {MessageId}", message.Id);
        }

        await UpdateMessageAsync(message.Id, error, connection, transaction);
    }

    private async Task ExecuteHandlersAsync(
        IIntegrationEvent integrationEvent,
        Type integrationEventType,
        CancellationToken cancellationToken
    )
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();

        IEnumerable<IIntegrationEventHandler> handlers =
            IntegrationEventHandlersFactory.GetHandlers(
                integrationEventType,
                scope.ServiceProvider,
                _presentationAssembly
            );

        foreach (IIntegrationEventHandler handler in handlers)
        {
            await handler.Handle(integrationEvent, cancellationToken);
        }
    }

    private async Task UpdateMessageAsync(
        Guid messageId,
        string? error,
        DbConnection connection,
        DbTransaction transaction
    )
    {
        string sql = $"""
            UPDATE {_schema}.inbox_messages
            SET processed_at_utc = @ProcessedAtUtc, error = @Error
            WHERE id = @Id;
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                Id = messageId,
                ProcessedAtUtc = DateTime.UtcNow,
                Error = error,
            },
            transaction
        );
    }

    private sealed record InboxMessageData(Guid Id, string Type, string Content);
}
