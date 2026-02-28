using Bogus;
using Deep.Accounts.Application.Data;
using Deep.Accounts.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Accounts.Application.Tests;

/// <summary>
/// Base class for Accounts integration tests.
/// </summary>
public abstract class AccountsIntegrationTestBase : IAsyncLifetime
{
    protected static readonly Faker Faker = new();
    private readonly IServiceScope _scope;
    protected readonly AccountsWebApplicationFactory Factory;
    protected readonly HttpClient HttpClient;

    protected AccountsDbContext DbContext =>
        _scope.ServiceProvider.GetRequiredService<AccountsDbContext>();

    protected AccountsIntegrationTestBase(AccountsWebApplicationFactory factory)
    {
        Factory = factory;
        _scope = factory.Services.CreateScope();
        HttpClient = factory.CreateClient();
    }

    public virtual async Task InitializeAsync()
    {
        // Apply migrations
        await DbContext.Database.MigrateAsync();

        // Seed roles if not exist (HasData doesn't always work with migrations in test scenarios)
        await SeedRolesAsync();

        await SeedDataAsync();
    }

    private async Task SeedRolesAsync()
    {
        // Check if roles already exist
        bool rolesExist = await DbContext.Set<Role>().AnyAsync();
        if (!rolesExist)
        {
            await DbContext.Database.ExecuteSqlRawAsync("""
                INSERT INTO accounts.roles (name) VALUES 
                    ('ItAdmin'),
                    ('ProgramOwner'),
                    ('Manager'),
                    ('BrandAmbassador'),
                    ('Coordinator')
                ON CONFLICT (name) DO NOTHING;
            """);
        }
    }

    public virtual async Task DisposeAsync()
    {
        await CleanupDataAsync();
        _scope.Dispose();
        HttpClient.Dispose();
    }

    /// <summary>
    /// Override to seed test data before each test.
    /// </summary>
    protected virtual Task SeedDataAsync() => Task.CompletedTask;

    /// <summary>
    /// Cleans up the database after each test (except reference data like roles).
    /// </summary>
    protected virtual async Task CleanupDataAsync()
    {
        // Only truncate data tables, not reference tables like roles
        await DbContext.Database.ExecuteSqlRawAsync("""
            TRUNCATE TABLE accounts.accounts CASCADE;
            TRUNCATE TABLE accounts.refresh_tokens CASCADE;
            TRUNCATE TABLE accounts.password_histories CASCADE;
            TRUNCATE TABLE accounts.password_reset_tokens CASCADE;
        """);
    }

    /// <summary>
    /// Creates a fresh scope with new DbContext instance.
    /// </summary>
    protected IServiceScope CreateFreshScope() => Factory.Services.CreateScope();
}
