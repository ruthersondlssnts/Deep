using Bogus;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Programs.Application.Tests;

/// <summary>
/// Base class for Programs integration tests.
/// </summary>
public abstract class ProgramsIntegrationTestBase : IAsyncLifetime
{
    protected static readonly Faker Faker = new();
    protected readonly ProgramsWebApplicationFactory Factory;
    protected readonly HttpClient HttpClient;

    protected ProgramsIntegrationTestBase(ProgramsWebApplicationFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
    }

    public virtual async Task InitializeAsync()
    {
        await using AsyncServiceScope scope = CreateAsyncScope();
        ProgramsDbContext db = scope.ServiceProvider.GetRequiredService<ProgramsDbContext>();
        await db.Database.MigrateAsync();
    }

    public virtual Task DisposeAsync()
    {
        HttpClient.Dispose();
        return Task.CompletedTask;
    }

    protected AsyncServiceScope CreateAsyncScope() => Factory.Services.CreateAsyncScope();

    protected IServiceScope CreateFreshScope() => Factory.Services.CreateScope();

    /// <summary>
    /// Invokes a handler directly via IRequestHandler.
    /// </summary>
    protected async Task<Result<TResponse>> SendAsync<TRequest, TResponse>(TRequest request)
    {
        await using AsyncServiceScope scope = CreateAsyncScope();
        IRequestHandler<TRequest, TResponse> handler = scope.ServiceProvider.GetRequiredService<
            IRequestHandler<TRequest, TResponse>
        >();
        return await handler.Handle(request);
    }

    /// <summary>
    /// Invokes a handler via IRequestBus.
    /// </summary>
    protected async Task<Result<TResponse>> SendViaBusAsync<TResponse>(object request)
    {
        await using AsyncServiceScope scope = CreateAsyncScope();
        IRequestBus bus = scope.ServiceProvider.GetRequiredService<IRequestBus>();
        return await bus.Send<TResponse>(request);
    }

    /// <summary>
    /// Seeds a test user in the Programs schema for program assignments.
    /// </summary>
    protected async Task<Guid> SeedTestUserAsync(string roleName)
    {
        var userId = Guid.CreateVersion7();
        await using AsyncServiceScope scope = CreateAsyncScope();
        ProgramsDbContext db = scope.ServiceProvider.GetRequiredService<ProgramsDbContext>();

        await db.Database.ExecuteSqlAsync(
            $@"
            INSERT INTO programs.users (id, email, first_name, last_name)
            VALUES ('{userId}', '{Faker.Internet.Email()}', '{Faker.Name.FirstName()}', '{Faker.Name.LastName()}')
            ON CONFLICT DO NOTHING;

            INSERT INTO programs.user_roles (user_id, role_name)
            VALUES ('{userId}', '{roleName}')
            ON CONFLICT DO NOTHING;
        "
        );

        return userId;
    }
}
