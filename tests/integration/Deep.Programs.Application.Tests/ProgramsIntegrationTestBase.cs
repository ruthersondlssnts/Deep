using System.Data.Common;
using System.Text.Json;
using Bogus;
using Dapper;
using Deep.Common.Application.Dapper;
using Deep.Common.Application.Database;
using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.BackgroundJobs;
using Deep.Programs.Application.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Programs.Application.Tests;

public abstract class ProgramsIntegrationTestBase
{
    protected static readonly Faker Faker = new();
    protected readonly ProgramsWebApplicationFactory Factory;
    protected readonly HttpClient HttpClient;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    protected ProgramsIntegrationTestBase(ProgramsWebApplicationFactory factory)
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

    protected async Task<Result<TResponse>> SendViaBusAsync<TResponse>(object request)
    {
        await using AsyncServiceScope scope = CreateAsyncScope();
        IRequestBus bus = scope.ServiceProvider.GetRequiredService<IRequestBus>();
        return await bus.Send<TResponse>(request);
    }

    #endregion

    #region Test Data Helpers

    protected async Task<Guid> SeedTestUserAsync(string roleName)
    {
        var userId = Guid.CreateVersion7();
        var email = Faker.Internet.Email();
        var firstName = Faker.Name.FirstName();
        var lastName = Faker.Name.LastName();

        await using DbConnection connection = await OpenConnectionAsync();

        await connection.ExecuteAsync(
            """
            INSERT INTO programs.users (id, email, first_name, last_name)
            VALUES (@UserId, @Email, @FirstName, @LastName)
            ON CONFLICT DO NOTHING;
            """,
            new { UserId = userId, Email = email, FirstName = firstName, LastName = lastName }
        );

        await connection.ExecuteAsync(
            """
            INSERT INTO programs.user_roles (user_id, role_name)
            VALUES (@UserId, @RoleName)
            ON CONFLICT DO NOTHING;
            """,
            new { UserId = userId, RoleName = roleName }
        );

        return userId;
    }

    #endregion

    #region Outbox Helpers

    protected async Task ProcessOutboxAsync()
    {
        await using AsyncServiceScope scope = CreateAsyncScope();
        ProgramsProcessOutboxJob job =
            scope.ServiceProvider.GetRequiredService<ProgramsProcessOutboxJob>();
        await job.ProcessAsync();
    }

    protected async Task<OutboxMessageRow?> GetOutboxMessageAsync(Guid messageId)
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            SELECT id AS Id, type AS Type, content AS Content, 
                   occurred_at_utc AS OccurredAtUtc, processed_at_utc AS ProcessedAtUtc, error AS Error
            FROM {Schemas.Programs}.outbox_messages
            WHERE id = @Id;
            """;

        return await connection.QuerySingleOrDefaultAsync<OutboxMessageRow>(
            sql,
            new { Id = messageId }
        );
    }

    protected async Task<IReadOnlyList<OutboxMessageRow>> GetUnprocessedOutboxMessagesAsync()
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            SELECT id AS Id, type AS Type, content AS Content, 
                   occurred_at_utc AS OccurredAtUtc, processed_at_utc AS ProcessedAtUtc, error AS Error
            FROM {Schemas.Programs}.outbox_messages
            WHERE processed_at_utc IS NULL
            ORDER BY occurred_at_utc;
            """;

        IEnumerable<OutboxMessageRow> messages =
            await connection.QueryAsync<OutboxMessageRow>(sql);
        return messages.ToList();
    }

    protected async Task<IReadOnlyList<OutboxMessageRow>> GetOutboxMessagesByTypeAsync(
        string typeName
    )
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            SELECT id AS Id, type AS Type, content AS Content, 
                   occurred_at_utc AS OccurredAtUtc, processed_at_utc AS ProcessedAtUtc, error AS Error
            FROM {Schemas.Programs}.outbox_messages
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

    #region Inbox Helpers

    protected async Task ProcessInboxAsync()
    {
        await using AsyncServiceScope scope = CreateAsyncScope();
        ProgramsProcessInboxJob job =
            scope.ServiceProvider.GetRequiredService<ProgramsProcessInboxJob>();
        await job.ProcessAsync();
    }

    protected async Task InsertInboxMessageAsync<TIntegrationEvent>(
        TIntegrationEvent integrationEvent
    )
        where TIntegrationEvent : IIntegrationEvent
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            INSERT INTO {Schemas.Programs}.inbox_messages (id, type, content, occurred_at_utc)
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

    protected async Task<InboxMessageRow?> GetInboxMessageAsync(Guid messageId)
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            SELECT id AS Id, type AS Type, content AS Content, 
                   occurred_at_utc AS OccurredAtUtc, processed_at_utc AS ProcessedAtUtc, error AS Error
            FROM {Schemas.Programs}.inbox_messages
            WHERE id = @Id;
            """;

        return await connection.QuerySingleOrDefaultAsync<InboxMessageRow>(
            sql,
            new { Id = messageId }
        );
    }

    protected async Task<IReadOnlyList<InboxMessageRow>> GetUnprocessedInboxMessagesAsync()
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            SELECT id AS Id, type AS Type, content AS Content, 
                   occurred_at_utc AS OccurredAtUtc, processed_at_utc AS ProcessedAtUtc, error AS Error
            FROM {Schemas.Programs}.inbox_messages
            WHERE processed_at_utc IS NULL
            ORDER BY occurred_at_utc;
            """;

        IEnumerable<InboxMessageRow> messages = await connection.QueryAsync<InboxMessageRow>(sql);
        return messages.ToList();
    }

    protected async Task<bool> InboxConsumerExistsAsync(Guid inboxMessageId, string consumerName)
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            SELECT COUNT(1) FROM {Schemas.Programs}.inbox_message_consumers
            WHERE inbox_message_id = @InboxMessageId AND name = @Name;
            """;

        int count = await connection.ExecuteScalarAsync<int>(
            sql,
            new { InboxMessageId = inboxMessageId, Name = consumerName }
        );

        return count > 0;
    }

    protected async Task<int> InsertInboxConsumerAsync(Guid inboxMessageId, string consumerName)
    {
        await using DbConnection connection = await OpenConnectionAsync();

        string sql = $"""
            INSERT INTO {Schemas.Programs}.inbox_message_consumers (inbox_message_id, name)
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

    protected async Task<DbConnection> OpenConnectionAsync()
    {
        await using AsyncServiceScope scope = CreateAsyncScope();
        IDbConnectionFactory connectionFactory =
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        return await connectionFactory.OpenConnectionAsync();
    }

    #endregion
}

public sealed record OutboxMessageRow(
    Guid Id,
    string Type,
    string Content,
    DateTime OccurredAtUtc,
    DateTime? ProcessedAtUtc,
    string? Error
)
{
    public T? DeserializeContent<T>()
        where T : class =>
        JsonSerializer.Deserialize<T>(
            Content,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
}

public sealed record InboxMessageRow(
    Guid Id,
    string Type,
    string Content,
    DateTime OccurredAtUtc,
    DateTime? ProcessedAtUtc,
    string? Error
)
{
    public T? DeserializeContent<T>()
        where T : class =>
        JsonSerializer.Deserialize<T>(
            Content,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
}

public sealed record InboxConsumerRow(Guid InboxMessageId, string Name);
