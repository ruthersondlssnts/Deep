using Deep.Common.Application.IntegrationEvents;
using Deep.Transactions.Application.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Deep.Transactions.Application.Tests;

public sealed class TransactionsWebApplicationFactory
    : WebApplicationFactory<Program>,
        IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("deep-db")
        .WithUsername("postgres")
        .Build();

    private bool _initialized;

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
            "mongodb://localhost:27017"
        );

        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<TransactionsDbContext>>();
            services.RemoveAll<TransactionsDbContext>();
            services.AddDbContext<TransactionsDbContext>(options =>
            {
                options.UseNpgsql(_postgres.GetConnectionString());
                options.UseSnakeCaseNamingConvention();
            });

            services.RemoveAll<Deep.Common.Application.Dapper.IDbConnectionFactory>();
            services.AddSingleton<Deep.Common.Application.Dapper.IDbConnectionFactory>(
                new TestDbConnectionFactory(_postgres.GetConnectionString())
            );

            services.RemoveAll<IEventBus>();
            services.AddSingleton<IEventBus, NoOpEventBus>();
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await EnsureDatabaseCreatedAsync();
    }

    public async Task EnsureDatabaseCreatedAsync()
    {
        if (_initialized)
            return;

        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();
        await db.Database.MigrateAsync();
        _initialized = true;
    }

    public new async Task DisposeAsync()
    {
        await _postgres.StopAsync();
        await base.DisposeAsync();
    }
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
    public Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : IIntegrationEvent => Task.CompletedTask;
}
