using Deep.Common.Application.IntegrationEvents;
using Deep.Programs.Application.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Npgsql;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;

namespace Deep.Programs.Application.Tests;

public sealed class ProgramsWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:latest")
        .WithDatabase("deep-db")
        .WithUsername("postgres")
        .Build();

    private readonly MongoDbContainer _mongo = new MongoDbBuilder("mongo:latest").Build();

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
            _mongo.GetConnectionString()
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
        });
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _mongo.StartAsync());
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

    public new async Task DisposeAsync()
    {
        await Task.WhenAll(_postgres.StopAsync(), _mongo.StopAsync());
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
