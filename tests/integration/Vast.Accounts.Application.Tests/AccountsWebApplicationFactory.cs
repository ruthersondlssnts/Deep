using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Vast.Accounts.Application.Data;
using Vast.Common.Application.Inbox;
using Vast.Common.Application.IntegrationEvents;
using Vast.Common.Application.Outbox;

namespace Vast.Accounts.Application.Tests;

public sealed class AccountsWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:latest")
        .WithDatabase("vast-db")
        .WithUsername("postgres")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder("redis:latest").Build();

    private bool _initialized;
    private bool _disposed;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__vast-db",
            _postgres.GetConnectionString()
        );
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__broker",
            "amqp://guest:guest@localhost:5672"
        );
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__vast-docs",
            "mongodb://localhost:27017"
        );
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__cache",
            _redis.GetConnectionString()
        );

        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AccountsDbContext>>();
            services.RemoveAll<AccountsDbContext>();
            services.AddDbContext<AccountsDbContext>(options =>
            {
                options.UseNpgsql(_postgres.GetConnectionString());
                options.UseSnakeCaseNamingConvention();
            });

            services.RemoveAll<Vast.Common.Application.Dapper.IDbConnectionFactory>();
            services.AddSingleton<Vast.Common.Application.Dapper.IDbConnectionFactory>(
                new TestDbConnectionFactory(_postgres.GetConnectionString())
            );

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
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());
        await EnsureDatabaseCreatedAsync();
    }

    public async Task EnsureDatabaseCreatedAsync()
    {
        if (_initialized)
        {
            return;
        }

        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        AccountsDbContext db = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
        await db.Database.MigrateAsync();
        _initialized = true;
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _postgres.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _redis.DisposeAsync().AsTask().GetAwaiter().GetResult();
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
            _disposed = true;
        }

        await base.DisposeAsync();
    }

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync().AsTask();
}

internal sealed class TestDbConnectionFactory(string connectionString)
    : Vast.Common.Application.Dapper.IDbConnectionFactory
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
