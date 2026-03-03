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

/// <summary>
/// WebApplicationFactory for Transactions integration tests with PostgreSQL Testcontainers.
/// </summary>
public sealed class TransactionsWebApplicationFactory
    : WebApplicationFactory<Program>,
        IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("deep-db")
        .WithUsername("postgres")
        .Build();

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
            // Replace TransactionsDbContext
            services.RemoveAll<DbContextOptions<TransactionsDbContext>>();
            services.RemoveAll<TransactionsDbContext>();
            services.AddDbContext<TransactionsDbContext>(options =>
            {
                options.UseNpgsql(_postgres.GetConnectionString());
                options.UseSnakeCaseNamingConvention();
            });

            // Replace IDbConnectionFactory
            services.RemoveAll<Deep.Common.Application.Dapper.IDbConnectionFactory>();
            services.AddSingleton<Deep.Common.Application.Dapper.IDbConnectionFactory>(
                new TestDbConnectionFactory(_postgres.GetConnectionString())
            );

            // Replace IEventBus with no-op
            services.RemoveAll<IEventBus>();
            services.AddSingleton<IEventBus, NoOpEventBus>();
        });
    }

    public async Task InitializeAsync() => await _postgres.StartAsync();

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
