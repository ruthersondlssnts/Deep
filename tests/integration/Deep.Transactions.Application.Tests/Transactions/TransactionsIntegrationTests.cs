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
    [Fact]
    public async Task CreateTransaction_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        CreateTransaction.Command request = new(
            Guid.CreateVersion7(),
            Faker.Internet.Email(),
            Faker.Name.FullName()
        );

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        CreateTransaction.Response? result = await response.Content.ReadFromJsonAsync<CreateTransaction.Response>();
        result.Should().NotBeNull();
        result!.TransactionId.Should().NotBeEmpty();
        result.CustomerId.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTransaction_WithNewCustomer_ShouldCreateCustomer()
    {
        // Arrange
        string customerEmail = Faker.Internet.Email();
        string customerName = Faker.Name.FullName();

        CreateTransaction.Command request = new(Guid.CreateVersion7(), customerEmail, customerName);

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using IServiceScope scope = CreateFreshScope();
        TransactionsDbContext dbContext = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();

        Customer? customer = await dbContext.Set<Customer>().FirstOrDefaultAsync(c => c.Email == customerEmail);
        customer.Should().NotBeNull();
        customer!.FullName.Should().Be(customerName);
    }

    [Fact]
    public async Task CreateTransaction_WithExistingCustomer_ShouldReuseCustomer()
    {
        // Arrange
        string customerEmail = Faker.Internet.Email();
        string customerName = Faker.Name.FullName();

        CreateTransaction.Command request1 = new(Guid.CreateVersion7(), customerEmail, customerName);
        CreateTransaction.Command request2 = new(Guid.CreateVersion7(), customerEmail, customerName);

        // Act
        HttpResponseMessage response1 = await HttpClient.PostAsJsonAsync("/transactions", request1);
        HttpResponseMessage response2 = await HttpClient.PostAsJsonAsync("/transactions", request2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        CreateTransaction.Response? result1 = await response1.Content.ReadFromJsonAsync<CreateTransaction.Response>();
        CreateTransaction.Response? result2 = await response2.Content.ReadFromJsonAsync<CreateTransaction.Response>();

        result1!.CustomerId.Should().Be(result2!.CustomerId);
        result1.TransactionId.Should().NotBe(result2.TransactionId);
    }

    [Fact]
    public async Task CreateTransaction_ShouldPersistToDatabase()
    {
        // Arrange
        CreateTransaction.Command request = new(Guid.CreateVersion7(), Faker.Internet.Email(), Faker.Name.FullName());

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/transactions", request);
        CreateTransaction.Response? result = await response.Content.ReadFromJsonAsync<CreateTransaction.Response>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using IServiceScope scope = CreateFreshScope();
        TransactionsDbContext dbContext = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();

        Transaction? savedTransaction = await dbContext.Set<Transaction>()
            .FirstOrDefaultAsync(t => t.Id == result!.TransactionId);

        savedTransaction.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTransaction_Handler_ShouldWork()
    {
        // Arrange
        CreateTransaction.Command command = new(
            Guid.CreateVersion7(),
            Faker.Internet.Email(),
            Faker.Name.FullName()
        );

        // Act
        Result<CreateTransaction.Response> result =
            await SendAsync<CreateTransaction.Command, CreateTransaction.Response>(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TransactionId.Should().NotBeEmpty();
    }
}
