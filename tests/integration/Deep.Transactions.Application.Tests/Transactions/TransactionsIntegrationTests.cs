using System.Net;
using System.Net.Http.Json;
using Deep.Common.Domain;
using Deep.Transactions.Application.Data;
using Deep.Transactions.Application.Features.Transactions;
using Deep.Transactions.Domain.Customer;
using Deep.Transactions.Domain.Transaction;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Transactions.Application.Tests.Transactions;

[Collection(nameof(TransactionsIntegrationCollection))]
public class TransactionsIntegrationTests(TransactionsWebApplicationFactory factory)
    : TransactionsIntegrationTestBase(factory)
{
    private const string TestSku = "TEST-SKU";
    private const string TestProductName = "Test Product";
    private const int TestQuantity = 2;
    private const decimal TestUnitPrice = 29.99m;

    [Fact]
    public async Task CreateTransaction_WithValidData_ShouldReturnCreated()
    {
        CreateTransaction.Command request = new(
            Guid.CreateVersion7(),
            TestSku,
            TestProductName,
            TestQuantity,
            TestUnitPrice,
            Faker.Internet.Email(),
            Faker.Name.FullName()
        );

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/transactions", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        CreateTransaction.Response? result =
            await response.Content.ReadFromJsonAsync<CreateTransaction.Response>();
        result.Should().NotBeNull();
        result!.TransactionId.Should().NotBeEmpty();
        result.CustomerId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateTransaction_WithValidData_ShouldCreateOutboxMessage()
    {
        CreateTransaction.Command request = new(
            Guid.CreateVersion7(),
            TestSku,
            TestProductName,
            TestQuantity,
            TestUnitPrice,
            Faker.Internet.Email(),
            Faker.Name.FullName()
        );

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/transactions", request);
        response.EnsureSuccessStatusCode();

        CreateTransaction.Response? result =
            await response.Content.ReadFromJsonAsync<CreateTransaction.Response>();

        IReadOnlyList<OutboxMessageRow> outboxMessages = await GetOutboxMessagesByTypeAsync(
            nameof(TransactionCreatedDomainEvent)
        );

        OutboxMessageRow? outboxMessage = outboxMessages.FirstOrDefault(m =>
        {
            TransactionCreatedDomainEvent? evt =
                m.DeserializeContent<TransactionCreatedDomainEvent>();
            return evt?.TransactionId == result!.TransactionId;
        });

        outboxMessage.Should().NotBeNull();
        outboxMessage!.ProcessedAtUtc.Should().BeNull();
        outboxMessage.Error.Should().BeNull();
    }

    [Fact]
    public async Task CreateTransaction_WhenOutboxProcessed_ShouldExecuteDomainEventHandler()
    {
        CreateTransaction.Command request = new(
            Guid.CreateVersion7(),
            TestSku,
            TestProductName,
            TestQuantity,
            TestUnitPrice,
            Faker.Internet.Email(),
            Faker.Name.FullName()
        );

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/transactions", request);
        response.EnsureSuccessStatusCode();

        CreateTransaction.Response? result =
            await response.Content.ReadFromJsonAsync<CreateTransaction.Response>();

        IReadOnlyList<OutboxMessageRow> messagesBefore = await GetOutboxMessagesByTypeAsync(
            nameof(TransactionCreatedDomainEvent)
        );
        OutboxMessageRow? messageBefore = messagesBefore.FirstOrDefault(m =>
        {
            TransactionCreatedDomainEvent? evt =
                m.DeserializeContent<TransactionCreatedDomainEvent>();
            return evt?.TransactionId == result!.TransactionId;
        });
        messageBefore.Should().NotBeNull();
        Guid messageId = messageBefore!.Id;

        await ProcessOutboxAsync();

        OutboxMessageRow? messageAfter = await GetOutboxMessageAsync(messageId);
        messageAfter.Should().NotBeNull();
        messageAfter!.ProcessedAtUtc.Should().NotBeNull();
        messageAfter.Error.Should().BeNull();
    }

    [Fact]
    public async Task CreateTransaction_WithNewCustomer_ShouldCreateCustomer()
    {
        string customerEmail = Faker.Internet.Email();
        string customerName = Faker.Name.FullName();
        CreateTransaction.Command request = new(
            Guid.CreateVersion7(),
            TestSku,
            TestProductName,
            TestQuantity,
            TestUnitPrice,
            customerEmail,
            customerName
        );

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/transactions", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using IServiceScope scope = CreateFreshScope();
        TransactionsDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();

        Customer? customer = await dbContext
            .Set<Customer>()
            .FirstOrDefaultAsync(c => c.Email == customerEmail);
        customer.Should().NotBeNull();
        customer!.FullName.Should().Be(customerName);
    }

    [Fact]
    public async Task CreateTransaction_WithExistingCustomer_ShouldReuseCustomer()
    {
        string customerEmail = Faker.Internet.Email();
        string customerName = Faker.Name.FullName();

        CreateTransaction.Command request1 = new(
            Guid.CreateVersion7(),
            TestSku,
            TestProductName,
            TestQuantity,
            TestUnitPrice,
            customerEmail,
            customerName
        );
        CreateTransaction.Command request2 = new(
            Guid.CreateVersion7(),
            TestSku,
            TestProductName,
            TestQuantity,
            TestUnitPrice,
            customerEmail,
            customerName
        );

        HttpResponseMessage response1 = await HttpClient.PostAsJsonAsync("/transactions", request1);
        HttpResponseMessage response2 = await HttpClient.PostAsJsonAsync("/transactions", request2);

        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        CreateTransaction.Response? result1 =
            await response1.Content.ReadFromJsonAsync<CreateTransaction.Response>();
        CreateTransaction.Response? result2 =
            await response2.Content.ReadFromJsonAsync<CreateTransaction.Response>();

        result1!.CustomerId.Should().Be(result2!.CustomerId);
        result1.TransactionId.Should().NotBe(result2.TransactionId);
    }

    [Fact]
    public async Task CreateTransaction_ShouldPersistToDatabase()
    {
        CreateTransaction.Command request = new(
            Guid.CreateVersion7(),
            TestSku,
            TestProductName,
            TestQuantity,
            TestUnitPrice,
            Faker.Internet.Email(),
            Faker.Name.FullName()
        );

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/transactions", request);
        CreateTransaction.Response? result =
            await response.Content.ReadFromJsonAsync<CreateTransaction.Response>();

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using IServiceScope scope = CreateFreshScope();
        TransactionsDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();

        Transaction? savedTransaction = await dbContext
            .Set<Transaction>()
            .FirstOrDefaultAsync(t => t.Id == result!.TransactionId);

        savedTransaction.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTransaction_Handler_ShouldWork()
    {
        CreateTransaction.Command command = new(
            Guid.CreateVersion7(),
            TestSku,
            TestProductName,
            TestQuantity,
            TestUnitPrice,
            Faker.Internet.Email(),
            Faker.Name.FullName()
        );

        Result<CreateTransaction.Response> result = await SendAsync<
            CreateTransaction.Command,
            CreateTransaction.Response
        >(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.TransactionId.Should().NotBeEmpty();
    }
}
