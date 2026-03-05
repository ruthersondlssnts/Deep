using System.Data.Common;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Dapper;
using Deep.Accounts.Application.BackgroundJobs;
using Deep.Accounts.Application.Data;
using Deep.Accounts.Application.Features.Accounts;
using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Accounts.Application.Tests;

public abstract class AccountsIntegrationTestBase
{
    protected static readonly Faker Faker = new();
    protected readonly AccountsWebApplicationFactory Factory;
    protected readonly HttpClient HttpClient;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    protected AccountsIntegrationTestBase(AccountsWebApplicationFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
    }

    #region Scope & Handler Helpers

    protected AsyncServiceScope CreateAsyncScope() => Factory.Services.CreateAsyncScope();

    protected IServiceScope CreateFreshScope() => Factory.Services.CreateScope();

    protected async Task<Result<TResponse>> SendAsync<TRequest, TResponse>(TRequest request)
    {
        await using AsyncServiceScope scope = CreateAsyncScope();
        IRequestHandler<TRequest, TResponse> handler = scope.ServiceProvider.GetRequiredService<
            IRequestHandler<TRequest, TResponse>
        >();
        return await handler.Handle(request);
    }

    /// <summary>
    /// Invokes a handler via IRequestBus (for features without endpoints).
    /// </summary>
    protected async Task<Result<TResponse>> SendViaBusAsync<TResponse>(object request)
    {
        await using AsyncServiceScope scope = CreateAsyncScope();
        IRequestBus bus = scope.ServiceProvider.GetRequiredService<IRequestBus>();
        return await bus.Send<TResponse>(request);
    }

    /// <summary>
    /// Resolves a service from the DI container.
    /// </summary>
    protected T GetRequiredService<T>()
        where T : notnull
    {
        using IServiceScope scope = CreateFreshScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    #endregion

    #region Test Data Helpers

    /// <summary>
    /// Creates a test account via the registration endpoint.
    /// </summary>
    protected async Task<RegisterAccountResponse> RegisterTestAccountAsync(
        string? email = null,
        string password = "Test1234!",
        IReadOnlyCollection<string>? roles = null
    )
    {
        RegisterAccountCommand request =
            new(
                Faker.Name.FirstName(),
                Faker.Name.LastName(),
                email ?? Faker.Internet.Email(),
                password,
                roles ?? [RoleNames.ItAdmin]
            );

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/accounts/register",
            request
        );
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<RegisterAccountResponse>())!;
    }

    #endregion

    #region Outbox Helpers (Manual Job Execution - No Hangfire)

    /// <summary>
    /// Manually executes the outbox processor job.
    /// Call this after an operation that raises domain events to process them.
    /// </summary>
    protected async Task ProcessOutboxAsync()
    {
        await using AsyncServiceScope scope = CreateAsyncScope();
        AccountsProcessOutboxJob job =
            scope.ServiceProvider.GetRequiredService<AccountsProcessOutboxJob>();
        await job.ProcessAsync();
    }

    /// <summary>
    /// Gets an outbox message by ID.
    /// </summary>
    protected async Task<OutboxMessageRow?> GetOutboxMessageAsync(Guid messageId)
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            SELECT id AS Id, type AS Type, content AS Content, 
                   occurred_at_utc AS OccurredAtUtc, processed_at_utc AS ProcessedAtUtc, error AS Error
            FROM {Schemas.Accounts}.outbox_messages
            WHERE id = @Id;
            """;

        return await connection.QuerySingleOrDefaultAsync<OutboxMessageRow>(
            sql,
            new { Id = messageId }
        );
    }

    /// <summary>
    /// Gets all unprocessed outbox messages.
    /// </summary>
    protected async Task<IReadOnlyList<OutboxMessageRow>> GetUnprocessedOutboxMessagesAsync()
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            SELECT id AS Id, type AS Type, content AS Content, 
                   occurred_at_utc AS OccurredAtUtc, processed_at_utc AS ProcessedAtUtc, error AS Error
            FROM {Schemas.Accounts}.outbox_messages
            WHERE processed_at_utc IS NULL
            ORDER BY occurred_at_utc;
            """;

        IEnumerable<OutboxMessageRow> messages =
            await connection.QueryAsync<OutboxMessageRow>(sql);
        return messages.ToList();
    }

    /// <summary>
    /// Gets outbox messages by type name.
    /// </summary>
    protected async Task<IReadOnlyList<OutboxMessageRow>> GetOutboxMessagesByTypeAsync(
        string typeName
    )
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            SELECT id AS Id, type AS Type, content AS Content, 
                   occurred_at_utc AS OccurredAtUtc, processed_at_utc AS ProcessedAtUtc, error AS Error
            FROM {Schemas.Accounts}.outbox_messages
            WHERE type = @Type
            ORDER BY occurred_at_utc DESC;
            """;

        IEnumerable<OutboxMessageRow> messages = await connection.QueryAsync<OutboxMessageRow>(
            sql,
            new { Type = typeName }
        );
        return messages.ToList();
    }

    #endregion

    #region Inbox Helpers (Manual Job Execution - No Hangfire)

    /// <summary>
    /// Manually executes the inbox processor job.
    /// Call this after inserting inbox messages to process integration events.
    /// </summary>
    protected async Task ProcessInboxAsync()
    {
        await using AsyncServiceScope scope = CreateAsyncScope();
        AccountsProcessInboxJob job =
            scope.ServiceProvider.GetRequiredService<AccountsProcessInboxJob>();
        await job.ProcessAsync();
    }

    /// <summary>
    /// Inserts an inbox message directly (simulates MassTransit consumer writing to inbox).
    /// </summary>
    protected async Task InsertInboxMessageAsync<TIntegrationEvent>(
        TIntegrationEvent integrationEvent
    )
        where TIntegrationEvent : IIntegrationEvent
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            INSERT INTO {Schemas.Accounts}.inbox_messages (id, type, content, occurred_at_utc)
            VALUES (@Id, @Type, @Content::jsonb, @OccurredAtUtc)
            ON CONFLICT (id) DO NOTHING;
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                integrationEvent.Id,
                Type = typeof(TIntegrationEvent).Name,
                Content = JsonSerializer.Serialize(
                    integrationEvent,
                    typeof(TIntegrationEvent),
                    JsonSerializerOptions
                ),
                integrationEvent.OccurredAtUtc,
            }
        );
    }

    /// <summary>
    /// Gets an inbox message by ID.
    /// </summary>
    protected async Task<InboxMessageRow?> GetInboxMessageAsync(Guid messageId)
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            SELECT id AS Id, type AS Type, content AS Content, 
                   occurred_at_utc AS OccurredAtUtc, processed_at_utc AS ProcessedAtUtc, error AS Error
            FROM {Schemas.Accounts}.inbox_messages
            WHERE id = @Id;
            """;

        return await connection.QuerySingleOrDefaultAsync<InboxMessageRow>(
            sql,
            new { Id = messageId }
        );
    }

    /// <summary>
    /// Gets all unprocessed inbox messages.
    /// </summary>
    protected async Task<IReadOnlyList<InboxMessageRow>> GetUnprocessedInboxMessagesAsync()
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            SELECT id AS Id, type AS Type, content AS Content, 
                   occurred_at_utc AS OccurredAtUtc, processed_at_utc AS ProcessedAtUtc, error AS Error
            FROM {Schemas.Accounts}.inbox_messages
            WHERE processed_at_utc IS NULL
            ORDER BY occurred_at_utc;
            """;

        IEnumerable<InboxMessageRow> messages = await connection.QueryAsync<InboxMessageRow>(sql);
        return messages.ToList();
    }

    /// <summary>
    /// Checks if an inbox consumer record exists (for idempotency verification).
    /// </summary>
    protected async Task<bool> InboxConsumerExistsAsync(Guid inboxMessageId, string consumerName)
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            SELECT COUNT(1) FROM {Schemas.Accounts}.inbox_message_consumers
            WHERE inbox_message_id = @InboxMessageId AND name = @Name;
            """;

        int count = await connection.ExecuteScalarAsync<int>(
            sql,
            new { InboxMessageId = inboxMessageId, Name = consumerName }
        );

        return count > 0;
    }

    /// <summary>
    /// Gets all inbox consumer records for a message.
    /// </summary>
    protected async Task<IReadOnlyList<InboxConsumerRow>> GetInboxConsumersAsync(
        Guid inboxMessageId
    )
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            SELECT inbox_message_id AS InboxMessageId, name AS Name
            FROM {Schemas.Accounts}.inbox_message_consumers
            WHERE inbox_message_id = @InboxMessageId;
            """;

        IEnumerable<InboxConsumerRow> consumers = await connection.QueryAsync<InboxConsumerRow>(
            sql,
            new { InboxMessageId = inboxMessageId }
        );

        return consumers.ToList();
    }

    /// <summary>
    /// Inserts a consumer record directly (for testing idempotency).
    /// </summary>
    protected async Task<int> InsertInboxConsumerAsync(Guid inboxMessageId, string consumerName)
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            INSERT INTO {Schemas.Accounts}.inbox_message_consumers (inbox_message_id, name)
            VALUES (@InboxMessageId, @Name)
            ON CONFLICT DO NOTHING;
            """;

        return await connection.ExecuteAsync(
            sql,
            new { InboxMessageId = inboxMessageId, Name = consumerName }
        );
    }

    #endregion

    #region Database Helpers

    /// <summary>
    /// Opens a database connection using the test connection factory.
    /// </summary>
    protected async Task<DbConnection> OpenConnectionAsync()
    {
        await using AsyncServiceScope scope = CreateAsyncScope();
        IDbConnectionFactory connectionFactory =
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        return await connectionFactory.OpenConnectionAsync();
    }

    /// <summary>
    /// Gets the AccountsDbContext from DI.
    /// </summary>
    protected async Task<AccountsDbContext> GetDbContextAsync()
    {
        AsyncServiceScope scope = CreateAsyncScope();
        return scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
    }

    /// <summary>
    /// Executes a raw SQL query and returns affected rows.
    /// </summary>
    protected async Task<int> ExecuteSqlAsync(string sql, object? param = null)
    {
        await using DbConnection connection = await OpenConnectionAsync();
        return await connection.ExecuteAsync(sql, param);
    }

    /// <summary>
    /// Queries the database and returns results.
    /// </summary>
    protected async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null)
    {
        await using DbConnection connection = await OpenConnectionAsync();
        IEnumerable<T> results = await connection.QueryAsync<T>(sql, param);
        return results.ToList();
    }

    #endregion
}

#region DTOs for Test Assertions

/// <summary>
/// DTO for outbox message rows.
/// </summary>
public sealed record OutboxMessageRow(
    Guid Id,
    string Type,
    string Content,
    DateTime OccurredAtUtc,
    DateTime? ProcessedAtUtc,
    string? Error
)
{
    /// <summary>
    /// Deserializes the content to the specified type.
    /// </summary>
    public T? DeserializeContent<T>()
        where T : class =>
        JsonSerializer.Deserialize<T>(
            Content,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
}

/// <summary>
/// DTO for inbox message rows.
/// </summary>
public sealed record InboxMessageRow(
    Guid Id,
    string Type,
    string Content,
    DateTime OccurredAtUtc,
    DateTime? ProcessedAtUtc,
    string? Error
)
{
    /// <summary>
    /// Deserializes the content to the specified type.
    /// </summary>
    public T? DeserializeContent<T>()
        where T : class =>
        JsonSerializer.Deserialize<T>(
            Content,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
}

/// <summary>
/// DTO for inbox consumer rows.
/// </summary>
public sealed record InboxConsumerRow(Guid InboxMessageId, string Name);

#endregion
