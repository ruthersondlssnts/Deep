using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text.Json;
using Dapper;
using Deep.Common.Application.Dapper;
using Deep.Common.Application.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deep.Common.Application.Inbox;

public interface IInboxProcessor
{
    Task<int> ProcessAsync(CancellationToken cancellationToken = default);
}

public abstract class InboxProcessorBase(
    IDbConnectionFactory connectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<InboxOptions> options,
    ILogger logger,
    string schema,
    Assembly presentationAssembly
) : IInboxProcessor
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

    public async Task<int> ProcessAsync(CancellationToken cancellationToken = default)
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
                await transaction.CommitAsync(cancellationToken);
                return 0;
            }

            List<(Guid Id, string? Error)> results = [];

            foreach (InboxMessageData message in messages)
            {
                string? error = await ProcessMessageAsync(message, cancellationToken);
                results.Add((message.Id, error));
            }

            await UpdateMessagesAsync(results, connection, transaction);

            await transaction.CommitAsync(cancellationToken);

            return messages.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Inbox processing failed for schema {Schema}", _schema);
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

    private async Task<string?> ProcessMessageAsync(
        InboxMessageData message,
        CancellationToken cancellationToken
    )
    {
        try
        {
            Type? integrationEventType = IntegrationEventHandlersFactory.GetIntegrationEventType(
                message.Type,
                _presentationAssembly
            );

            integrationEventType ??= Type.GetType(message.Type);

            if (integrationEventType is null)
            {
                return $"Integration event type not found: {message.Type}";
            }

            object? deserialized = JsonSerializer.Deserialize(
                message.Content,
                integrationEventType,
                JsonSerializerOptions
            );

            if (deserialized is not IIntegrationEvent integrationEvent)
            {
                return $"Failed to deserialize integration event: {message.Type}";
            }

            await ExecuteHandlersAsync(integrationEvent, integrationEventType, cancellationToken);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing inbox message {Id}", message.Id);
            return ex.ToString();
        }
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

        Task[] tasks = handlers
            .Select(handler => handler.Handle(integrationEvent, cancellationToken))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    private async Task UpdateMessagesAsync(
        IReadOnlyCollection<(Guid Id, string? Error)> messages,
        DbConnection connection,
        DbTransaction transaction
    )
    {
        string sql = $"""
            UPDATE {_schema}.inbox_messages
            SET processed_at_utc = @ProcessedAtUtc,
                error = @Error
            WHERE id = @Id;
            """;

        await connection.ExecuteAsync(
            sql,
            messages.Select(m => new
            {
                m.Id,
                ProcessedAtUtc = DateTime.UtcNow,
                m.Error,
            }),
            transaction
        );
    }

    private sealed record InboxMessageData(Guid Id, string Type, string Content);
}

public abstract class InboxBackgroundService<TProcessor>(
    IServiceScopeFactory scopeFactory,
    IInboxNotifier notifier,
    IOptions<InboxOptions> options,
    ILogger logger,
    string moduleName
) : BackgroundService
    where TProcessor : class, IInboxProcessor
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IInboxNotifier _notifier = notifier;
    private readonly InboxOptions _options = options.Value;
    private readonly ILogger _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Inbox worker started for module {Module}", moduleName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delayTask = Task.Delay(
                    TimeSpan.FromSeconds(_options.IntervalInSeconds),
                    stoppingToken
                );

                Task signalTask = _notifier.WaitAsync(stoppingToken);

                await Task.WhenAny(delayTask, signalTask);

                await DrainAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Inbox worker failure");

                await Task.Delay(TimeSpan.FromSeconds(_options.ErrorDelayInSeconds), stoppingToken);
            }
        }
    }

    private async Task DrainAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();

            TProcessor processor = scope.ServiceProvider.GetRequiredService<TProcessor>();

            int processed = await processor.ProcessAsync(token);

            if (processed == 0)
            {
                return;
            }
        }
    }
}
