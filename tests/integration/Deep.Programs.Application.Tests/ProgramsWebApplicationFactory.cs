using Deep.Common.Application.Inbox;
using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Application.Outbox;
using Deep.Programs.Application.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Npgsql;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Deep.Programs.Application.Tests;

public sealed class ProgramsWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:latest")
        .WithDatabase("deep-db")
        .WithUsername("postgres")
        .Build();

    private readonly MongoDbContainer _mongo = new MongoDbBuilder("mongo:latest").Build();

    private readonly RedisContainer _redis = new RedisBuilder("redis:latest").Build();

    private bool _initialized;
    private bool _disposed;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__deep-db",
            _postgres.GetConnectionString()
        );
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__broker",
            "amqp://guest:guest@localhost:5672"
        );
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__deep-docs",
            _mongo.GetConnectionString()
        );
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__cache",
            _redis.GetConnectionString()
        );

        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ProgramsDbContext>>();
            services.RemoveAll<ProgramsDbContext>();
            services.AddDbContext<ProgramsDbContext>(options =>
            {
                options.UseNpgsql(_postgres.GetConnectionString());
                options.UseSnakeCaseNamingConvention();
            });

            services.RemoveAll<Deep.Common.Application.Dapper.IDbConnectionFactory>();
            services.AddSingleton<Deep.Common.Application.Dapper.IDbConnectionFactory>(
                new TestDbConnectionFactory(_postgres.GetConnectionString())
            );

            services.RemoveAll<MongoDbContext>();
            services.AddSingleton(_ =>
            {
                MongoClient client = new(_mongo.GetConnectionString());
                IMongoDatabase database = client.GetDatabase("deep-docs");
                return new MongoDbContext(database);
            });

            services.RemoveAll<IEventBus>();
            services.AddSingleton<IEventBus, NoOpEventBus>();

            // Remove outbox/inbox background workers to prevent race conditions in tests
            foreach (
                ServiceDescriptor descriptor in services
                    .Where(d =>
                        d.ServiceType == typeof(IHostedService)
                        && d.ImplementationType?.BaseType is { IsGenericType: true } bt
                        && (
                            bt.GetGenericTypeDefinition() == typeof(OutboxBackgroundService<>)
                            || bt.GetGenericTypeDefinition() == typeof(InboxBackgroundService<>)
                        )
                    )
                    .ToList()
            )
            {
                services.Remove(descriptor);
            }
        });
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _mongo.StartAsync(), _redis.StartAsync());
        await EnsureDatabaseCreatedAsync();
    }

    public async Task EnsureDatabaseCreatedAsync()
    {
        if (_initialized)
        {
            return;
        }

        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        ProgramsDbContext db = scope.ServiceProvider.GetRequiredService<ProgramsDbContext>();
        await db.Database.MigrateAsync();
        _initialized = true;
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _postgres.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _redis.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _mongo.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _disposed = true;
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _postgres.DisposeAsync();
            await _redis.DisposeAsync();
            await _mongo.DisposeAsync();
            _disposed = true;
        }

        await base.DisposeAsync();
    }

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync().AsTask();
}

internal sealed class TestDbConnectionFactory(string connectionString)
    : Deep.Common.Application.Dapper.IDbConnectionFactory
{
    public async ValueTask<System.Data.Common.DbConnection> OpenConnectionAsync()
    {
        NpgsqlConnection connection = new(connectionString);
        await connection.OpenAsync();
        return connection;
    }
}

internal sealed class NoOpEventBus : IEventBus
{
    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent => Task.CompletedTask;
}
