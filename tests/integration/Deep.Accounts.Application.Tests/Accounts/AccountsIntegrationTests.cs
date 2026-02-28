using System.Net;
using System.Net.Http.Json;
using Deep.Accounts.Application.Features.Accounts;
using Deep.Common.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Accounts.Application.Tests.Accounts;

[Collection(nameof(AccountsIntegrationCollection))]
public class AccountsIntegrationTests(AccountsWebApplicationFactory factory) 
    : AccountsIntegrationTestBase(factory)
{
    [Fact]
    public async Task RegisterAccount_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new RegisterAccountCommand(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            Faker.Internet.Email(),
            Faker.Internet.Password(prefix: "Test1234!"),
            [RoleNames.ItAdmin]);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/accounts/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<RegisterAccountResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RegisterAccount_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        var email = Faker.Internet.Email();
        var request = new RegisterAccountCommand(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            email,
            Faker.Internet.Password(prefix: "Test1234!"),
            [RoleNames.ItAdmin]);

        // First registration should succeed
        var firstResponse = await HttpClient.PostAsJsonAsync("/accounts/register", request);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Second registration with same email
        var secondRequest = new RegisterAccountCommand(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            email,
            Faker.Internet.Password(prefix: "Test1234!"),
            [RoleNames.Manager]);

        var secondResponse = await HttpClient.PostAsJsonAsync("/accounts/register", secondRequest);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RegisterAccount_WithInvalidRole_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterAccountCommand(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            Faker.Internet.Email(),
            Faker.Internet.Password(prefix: "Test1234!"),
            ["InvalidRole"]);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/accounts/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterAccount_WithMultipleRoles_ShouldReturnCreated()
    {
        // Arrange
        var request = new RegisterAccountCommand(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            Faker.Internet.Email(),
            Faker.Internet.Password(prefix: "Test1234!"),
            [RoleNames.ItAdmin, RoleNames.Manager]);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/accounts/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetAccounts_WithRegisteredAccounts_ShouldReturnAllAccounts()
    {
        // Arrange
        var account1 = new RegisterAccountCommand(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            Faker.Internet.Email(),
            Faker.Internet.Password(prefix: "Test1234!"),
            [RoleNames.ItAdmin]);

        var account2 = new RegisterAccountCommand(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            Faker.Internet.Email(),
            Faker.Internet.Password(prefix: "Test1234!"),
            [RoleNames.Manager]);

        await HttpClient.PostAsJsonAsync("/accounts/register", account1);
        await HttpClient.PostAsJsonAsync("/accounts/register", account2);

        // Act
        var response = await HttpClient.GetAsync("/accounts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var accounts = await response.Content.ReadFromJsonAsync<List<GetAccounts.Response>>();
        accounts.Should().NotBeNull();
        accounts!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAccounts_FilterByRole_ShouldReturnFilteredAccounts()
    {
        // Arrange
        var itAdminAccount = new RegisterAccountCommand(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            Faker.Internet.Email(),
            Faker.Internet.Password(prefix: "Test1234!"),
            [RoleNames.ItAdmin]);

        var managerAccount = new RegisterAccountCommand(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            Faker.Internet.Email(),
            Faker.Internet.Password(prefix: "Test1234!"),
            [RoleNames.Manager]);

        await HttpClient.PostAsJsonAsync("/accounts/register", itAdminAccount);
        await HttpClient.PostAsJsonAsync("/accounts/register", managerAccount);

        // Act
        var response = await HttpClient.GetAsync($"/accounts?role={RoleNames.ItAdmin}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var accounts = await response.Content.ReadFromJsonAsync<List<GetAccounts.Response>>();
        accounts.Should().NotBeNull();
        accounts!.Should().OnlyContain(a => a.Roles.Contains(RoleNames.ItAdmin));
    }

    [Fact]
    public async Task RegisterAccount_ShouldPersistToDatabase()
    {
        // Arrange
        var email = Faker.Internet.Email();
        var firstName = Faker.Name.FirstName();
        var lastName = Faker.Name.LastName();

        var request = new RegisterAccountCommand(
            firstName,
            lastName,
            email,
            Faker.Internet.Password(prefix: "Test1234!"),
            [RoleNames.ItAdmin]);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/accounts/register", request);
        var result = await response.Content.ReadFromJsonAsync<RegisterAccountResponse>();

        // Assert - Verify in database
        using var scope = CreateFreshScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Deep.Accounts.Application.Data.AccountsDbContext>();

        var savedAccount = await dbContext.Set<Deep.Accounts.Domain.Accounts.Account>()
            .FirstOrDefaultAsync(a => a.Id == result!.Id);

        savedAccount.Should().NotBeNull();
        savedAccount!.Email.Should().Be(email);
        savedAccount.FirstName.Should().Be(firstName);
        savedAccount.LastName.Should().Be(lastName);
        savedAccount.IsActive.Should().BeTrue();
    }
}
