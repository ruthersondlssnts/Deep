using System.Net;
using System.Net.Http.Json;
using Deep.Testing.Integration;
using Deep.Transactions.Application.Features.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Transactions.Application.Tests.Transactions;

[Collection(nameof(TransactionsIntegrationCollection))]
public class TransactionsIntegrationTests(DeepWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateTransaction_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateTransaction.Command(
            Guid.CreateVersion7(), // ProgramId
            Faker.Internet.Email(),
            Faker.Name.FullName());

        // Act
        var response = await HttpClient.PostAsJsonAsync("/transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateTransaction.Response>();
        result.Should().NotBeNull();
        result!.TransactionId.Should().NotBeEmpty();
        result.CustomerId.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTransaction_WithNewCustomer_ShouldCreateCustomer()
    {
        // Arrange
        var customerEmail = Faker.Internet.Email();
        var customerName = Faker.Name.FullName();
        var programId = Guid.CreateVersion7();

        var request = new CreateTransaction.Command(
            programId,
            customerEmail,
            customerName);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify customer was created in database
        using var scope = CreateFreshScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Deep.Transactions.Application.Data.TransactionsDbContext>();

        var customer = await dbContext.Set<Deep.Transactions.Domain.Customer.Customer>()
            .FirstOrDefaultAsync(c => c.Email == customerEmail);

        customer.Should().NotBeNull();
        customer!.FullName.Should().Be(customerName);
    }

    [Fact]
    public async Task CreateTransaction_WithExistingCustomer_ShouldReuseCustomer()
    {
        // Arrange
        var customerEmail = Faker.Internet.Email();
        var customerName = Faker.Name.FullName();
        var programId1 = Guid.CreateVersion7();
        var programId2 = Guid.CreateVersion7();

        var request1 = new CreateTransaction.Command(programId1, customerEmail, customerName);
        var request2 = new CreateTransaction.Command(programId2, customerEmail, customerName);

        // Act
        var response1 = await HttpClient.PostAsJsonAsync("/transactions", request1);
        var response2 = await HttpClient.PostAsJsonAsync("/transactions", request2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        var result1 = await response1.Content.ReadFromJsonAsync<CreateTransaction.Response>();
        var result2 = await response2.Content.ReadFromJsonAsync<CreateTransaction.Response>();

        // Both transactions should reference the same customer
        result1!.CustomerId.Should().Be(result2!.CustomerId);
        result1.TransactionId.Should().NotBe(result2.TransactionId);
    }

    [Fact]
    public async Task CreateTransaction_ShouldPersistToDatabase()
    {
        // Arrange
        var programId = Guid.CreateVersion7();
        var customerEmail = Faker.Internet.Email();

        var request = new CreateTransaction.Command(
            programId,
            customerEmail,
            Faker.Name.FullName());

        // Act
        var response = await HttpClient.PostAsJsonAsync("/transactions", request);
        var result = await response.Content.ReadFromJsonAsync<CreateTransaction.Response>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = CreateFreshScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Deep.Transactions.Application.Data.TransactionsDbContext>();

        var savedTransaction = await dbContext.Set<Deep.Transactions.Domain.Transaction.Transaction>()
            .FirstOrDefaultAsync(t => t.Id == result!.TransactionId);

        savedTransaction.Should().NotBeNull();
        savedTransaction!.ProgramId.Should().Be(programId);
        savedTransaction.CustomerId.Should().Be(result!.CustomerId!.Value);
    }

    [Fact]
    public async Task CreateMultipleTransactions_ShouldAllSucceed()
    {
        // Arrange
        var requests = Enumerable.Range(1, 5)
            .Select(_ => new CreateTransaction.Command(
                Guid.CreateVersion7(),
                Faker.Internet.Email(),
                Faker.Name.FullName()))
            .ToList();

        // Act
        var responses = await Task.WhenAll(
            requests.Select(r => HttpClient.PostAsJsonAsync("/transactions", r)));

        // Assert
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.Created);

        // Verify all transactions are in database
        using var scope = CreateFreshScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Deep.Transactions.Application.Data.TransactionsDbContext>();

        var transactionCount = await dbContext.Set<Deep.Transactions.Domain.Transaction.Transaction>().CountAsync();
        transactionCount.Should().BeGreaterThanOrEqualTo(5);
    }
}
