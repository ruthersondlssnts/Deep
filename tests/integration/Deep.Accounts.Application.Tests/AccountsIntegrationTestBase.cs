using Bogus;
using Deep.Accounts.Application.Data;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Accounts.Application.Tests;

public abstract class AccountsIntegrationTestBase : IAsyncLifetime
{
    protected static readonly Faker Faker = new();
    protected readonly AccountsWebApplicationFactory Factory;
    protected readonly HttpClient HttpClient;

    protected AccountsIntegrationTestBase(AccountsWebApplicationFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
    }

    public virtual async Task InitializeAsync()
    {
        await using AsyncServiceScope scope = CreateAsyncScope();
        AccountsDbContext db = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
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
}
