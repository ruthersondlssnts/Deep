using System.Data.Common;
using System.Text.Json;
using Dapper;
using Deep.Common.Application.Dapper;
using Deep.Common.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace Deep.Common.Application.Inbox;

public abstract class InboxWriterBase(
    IDbConnectionFactory connectionFactory,
    ILogger logger,
    string schema
)
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    private readonly ILogger _logger = logger;
    private readonly string _schema = schema;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public async Task WriteAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent)
        where TIntegrationEvent : IIntegrationEvent
    {
        await using DbConnection connection = await _connectionFactory.OpenConnectionAsync();

        string sql = $"""
            INSERT INTO {_schema}.inbox_messages (id, type, content, occurred_at_utc)
            VALUES (@Id, @Type, @Content::jsonb, @OccurredAtUtc)
            ON CONFLICT (id) DO NOTHING;
            """;

        int affectedRows = await connection.ExecuteAsync(
            sql,
            new
            {
                integrationEvent.Id,
                Type = integrationEvent.GetType().AssemblyQualifiedName,
                Content = JsonSerializer.Serialize(
                    integrationEvent,
                    integrationEvent.GetType(),
                    JsonSerializerOptions
                ),
                integrationEvent.OccurredAtUtc,
            }
        );

        if (affectedRows > 0)
        {
            _logger.LogDebug(
                "Wrote integration event {EventType} with Id {EventId} to inbox in schema {Schema}",
                integrationEvent.GetType().Name,
                integrationEvent.Id,
                _schema
            );
        }
        else
        {
            _logger.LogDebug(
                "Integration event {EventType} with Id {EventId} already exists in inbox in schema {Schema}",
                integrationEvent.GetType().Name,
                integrationEvent.Id,
                _schema
            );
        }
    }
}
