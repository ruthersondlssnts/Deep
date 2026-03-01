using Bogus;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Transactions.Application.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Transactions.Application.Tests;

/// <summary>
/// Base class for Transactions integration tests.
/// </summary>
public abstract class TransactionsIntegrationTestBase : IAsyncLifetime
{
    protected static readonly Faker Faker = new();
    protected readonly TransactionsWebApplicationFactory Factory;
    protected readonly HttpClient HttpClient;

    protected TransactionsIntegrationTestBase(TransactionsWebApplicationFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
    }

    public virtual async Task InitializeAsync()
    {
        await using AsyncServiceScope scope = CreateAsyncScope();
        TransactionsDbContext db = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();
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
        IRequestHandler<TRequest, TResponse> handler =
            scope.ServiceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        return await handler.Handle(request);
    }
}
