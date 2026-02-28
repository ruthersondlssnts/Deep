using Deep.Accounts.Application.Data;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Deep.Accounts.Application.Tests;

/// <summary>
/// Custom WebApplicationFactory for Accounts integration testing with Testcontainers.
/// Implements IAsyncLifetime to manage container lifecycle.
/// </summary>
public class AccountsWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("deep_accounts_test")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

    public string PostgreSqlConnectionString => _postgreSqlContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment variables for connection strings before app starts
        Environment.SetEnvironmentVariable("ConnectionStrings__deep-db", _postgreSqlContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__broker", "amqp://guest:guest@localhost:5672");
        Environment.SetEnvironmentVariable("ConnectionStrings__deep-docs", "mongodb://localhost:27017");

        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Remove existing AccountsDbContext registration
            ServiceDescriptor? dbContextDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<AccountsDbContext>));
            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }
            services.RemoveAll<AccountsDbContext>();

            // Re-register with test container connection string
            string connectionString = _postgreSqlContainer.GetConnectionString();
            services.AddDbContext<AccountsDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.UseSnakeCaseNamingConvention();
            });

            // Replace IDbConnectionFactory
            services.RemoveAll<Deep.Common.Application.Dapper.IDbConnectionFactory>();
            services.AddSingleton<Deep.Common.Application.Dapper.IDbConnectionFactory>(
                new TestDbConnectionFactory(connectionString));
        });
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgreSqlContainer.StopAsync();
        await base.DisposeAsync();
    }
}

/// <summary>
/// Test implementation of IDbConnectionFactory.
/// </summary>
public class TestDbConnectionFactory(string connectionString) 
    : Deep.Common.Application.Dapper.IDbConnectionFactory
{
    public async ValueTask<System.Data.Common.DbConnection> OpenConnectionAsync()
    {
        NpgsqlConnection connection = new(connectionString);
        await connection.OpenAsync();
        return connection;
    }
}
