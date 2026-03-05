using System.Net;
using System.Net.Http.Json;
using Deep.Accounts.Application.Data;
using Deep.Accounts.Application.Features.Accounts;
using Deep.Accounts.Application.Features.Authentication;
using Deep.Accounts.Application.Features.Passwords;
using Deep.Accounts.Domain.Accounts;
using Deep.Common.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Accounts.Application.Tests.Accounts;

/// <summary>
/// Feature-oriented integration tests for Account features.
/// 
/// Strategy:
/// - Features with endpoints → HTTP call → Assert response + DB + outbox
/// - Features without endpoints → Handler call → Assert Result + DB
/// - Outbox processing → Manual job execution (no Hangfire)
/// </summary>
[Collection(nameof(AccountsIntegrationCollection))]
public class AccountsIntegrationTests(AccountsWebApplicationFactory factory)
    : AccountsIntegrationTestBase(factory)
{
    #region RegisterAccount Feature (Endpoint + Outbox)

    [Fact]
    public async Task RegisterAccount_WithValidData_ShouldReturnCreated_AndCreateOutboxMessage()
    {
        // Arrange
        RegisterAccountCommand request = new(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            Faker.Internet.Email(),
            "Test1234!",
            [RoleNames.ItAdmin]
        );

        // Act - Call endpoint
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/accounts/register",
            request
        );

        // Assert - HTTP Response
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        RegisterAccountResponse? result =
            await response.Content.ReadFromJsonAsync<RegisterAccountResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();

        // Assert - Outbox Message Created (before processing)
        IReadOnlyList<OutboxMessageRow> outboxMessages = await GetOutboxMessagesByTypeAsync(
            nameof(AccountRegisteredDomainEvent)
        );
        OutboxMessageRow? outboxMessage = outboxMessages.FirstOrDefault(m =>
        {
            AccountRegisteredDomainEvent? evt = m.DeserializeContent<AccountRegisteredDomainEvent>();
            return evt?.AccountId == result.Id;
        });

        outboxMessage.Should().NotBeNull();
        outboxMessage!.ProcessedAtUtc.Should().BeNull("outbox message should not be processed yet");
        outboxMessage.Error.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAccount_WithValidData_WhenOutboxProcessed_ShouldExecuteDomainEventHandler()
    {
        // Arrange
        RegisterAccountCommand request = new(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            Faker.Internet.Email(),
            "Test1234!",
            [RoleNames.ItAdmin]
        );

        // Act - Call endpoint
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/accounts/register",
            request
        );
        response.EnsureSuccessStatusCode();
        RegisterAccountResponse? result =
            await response.Content.ReadFromJsonAsync<RegisterAccountResponse>();

        // Get outbox message before processing
        IReadOnlyList<OutboxMessageRow> outboxMessagesBefore = await GetOutboxMessagesByTypeAsync(
            nameof(AccountRegisteredDomainEvent)
        );
        OutboxMessageRow? outboxMessageBefore = outboxMessagesBefore.FirstOrDefault(m =>
        {
            AccountRegisteredDomainEvent? evt = m.DeserializeContent<AccountRegisteredDomainEvent>();
            return evt?.AccountId == result!.Id;
        });
        outboxMessageBefore.Should().NotBeNull();
        Guid outboxMessageId = outboxMessageBefore!.Id;

        // Act - Manually execute outbox processor (NO Hangfire)
        await ProcessOutboxAsync();

        // Assert - Outbox message processed
        OutboxMessageRow? outboxMessageAfter = await GetOutboxMessageAsync(outboxMessageId);
        outboxMessageAfter.Should().NotBeNull();
        outboxMessageAfter!.ProcessedAtUtc.Should().NotBeNull("outbox message should be processed");
        outboxMessageAfter.Error.Should().BeNull("no error should occur during processing");

        // Assert - Domain event handler side effect executed
        // (AccountRegisteredDomainEventHandler publishes AccountRegisteredIntegrationEvent via IEventBus)
        // In tests, IEventBus is replaced with NoOpEventBus, so we just verify no errors
    }

    [Fact]
    public async Task RegisterAccount_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        string email = Faker.Internet.Email();
        RegisterAccountCommand request = new(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            email,
            "Test1234!",
            [RoleNames.ItAdmin]
        );

        await HttpClient.PostAsJsonAsync("/accounts/register", request);

        // Act
        RegisterAccountCommand duplicateRequest = new(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            email,
            "Test1234!",
            [RoleNames.Manager]
        );
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/accounts/register",
            duplicateRequest
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RegisterAccount_WithInvalidRole_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        RegisterAccountCommand request = new(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            Faker.Internet.Email(),
            "Test1234!",
            ["InvalidRole"]
        );

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/accounts/register",
            request
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task RegisterAccount_WithMultipleRoles_ShouldReturnCreated()
    {
        // Arrange
        RegisterAccountCommand request = new(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            Faker.Internet.Email(),
            "Test1234!",
            [RoleNames.ItAdmin, RoleNames.Manager]
        );

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/accounts/register",
            request
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion

    #region Login Feature (Endpoint)

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        string email = Faker.Internet.Email();
        string password = "Test1234!";
        await RegisterTestAccountAsync(email, password);

        LoginAccount.Command loginRequest = new(email, password);

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/accounts/login",
            loginRequest
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        LoginAccount.Response? result =
            await response.Content.ReadFromJsonAsync<LoginAccount.Response>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnForbidden()
    {
        // Arrange
        string email = Faker.Internet.Email();
        await RegisterTestAccountAsync(email, "Test1234!");

        LoginAccount.Command loginRequest = new(email, "WrongPassword!");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/accounts/login",
            loginRequest
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region RefreshToken Feature (Endpoint)

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        string email = Faker.Internet.Email();
        string password = "Test1234!";
        await RegisterTestAccountAsync(email, password);

        HttpResponseMessage loginResponse = await HttpClient.PostAsJsonAsync(
            "/accounts/login",
            new LoginAccount.Command(email, password)
        );
        LoginAccount.Response? loginResult =
            await loginResponse.Content.ReadFromJsonAsync<LoginAccount.Response>();

        RefreshAccessToken.Command refreshRequest = new(loginResult!.RefreshToken);

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/accounts/refresh-token",
            refreshRequest
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshAccessToken.Response? result =
            await response.Content.ReadFromJsonAsync<RefreshAccessToken.Response>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe(loginResult.RefreshToken);
    }

    #endregion

    #region Logout Feature (Endpoint)

    [Fact]
    public async Task Logout_WithValidToken_ShouldSucceed()
    {
        // Arrange
        string email = Faker.Internet.Email();
        string password = "Test1234!";
        await RegisterTestAccountAsync(email, password);

        HttpResponseMessage loginResponse = await HttpClient.PostAsJsonAsync(
            "/accounts/login",
            new LoginAccount.Command(email, password)
        );
        LoginAccount.Response? loginResult =
            await loginResponse.Content.ReadFromJsonAsync<LoginAccount.Response>();

        LogoutAccount.Command logoutRequest = new(loginResult!.RefreshToken);

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/accounts/logout",
            logoutRequest
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region ForgotPassword Feature (Endpoint)

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ShouldReturnResetToken()
    {
        // Arrange
        string email = Faker.Internet.Email();
        await RegisterTestAccountAsync(email, "Test1234!");

        ForgotPassword.Command request = new(email);

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/accounts/forgot-password",
            request
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ForgotPassword.Response? result =
            await response.Content.ReadFromJsonAsync<ForgotPassword.Response>();
        result.Should().NotBeNull();
        result!.ResetToken.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region ResetPassword Feature (Endpoint)

    [Fact]
    public async Task ResetPassword_WithValidToken_ShouldSucceed()
    {
        // Arrange
        string email = Faker.Internet.Email();
        await RegisterTestAccountAsync(email, "Test1234!");

        HttpResponseMessage forgotResponse = await HttpClient.PostAsJsonAsync(
            "/accounts/forgot-password",
            new ForgotPassword.Command(email)
        );
        ForgotPassword.Response? forgotResult =
            await forgotResponse.Content.ReadFromJsonAsync<ForgotPassword.Response>();

        ResetPassword.Command resetRequest = new(forgotResult!.ResetToken, "NewPassword1234!");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/accounts/reset-password",
            resetRequest
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify can login with new password
        HttpResponseMessage loginResponse = await HttpClient.PostAsJsonAsync(
            "/accounts/login",
            new LoginAccount.Command(email, "NewPassword1234!")
        );
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GetAccounts Feature (Endpoint)

    [Fact]
    public async Task GetAccounts_ShouldReturnAllAccounts()
    {
        // Arrange
        await RegisterTestAccountAsync(Faker.Internet.Email(), "Test1234!");
        await RegisterTestAccountAsync(Faker.Internet.Email(), "Test1234!");

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/accounts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<GetAccounts.Response>? accounts = await response.Content.ReadFromJsonAsync<
            List<GetAccounts.Response>
        >();
        accounts.Should().NotBeNull();
        accounts!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    #endregion

    #region GetAccount Feature (No Endpoint - Handler Test)

    [Fact]
    public async Task GetAccount_Handler_WithValidId_ShouldReturnAccount()
    {
        // Arrange - Create account via endpoint
        string email = Faker.Internet.Email();
        RegisterAccountResponse registered = await RegisterTestAccountAsync(email);

        // Act - Call handler directly (no endpoint for this feature)
        Result<GetAccount.Response> result = await SendAsync<GetAccount.Query, GetAccount.Response>(
            new GetAccount.Query(registered.Id)
        );

        // Assert - Result
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(email);
        result.Value.Id.Should().Be(registered.Id);
    }

    [Fact]
    public async Task GetAccount_Handler_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentId = Guid.CreateVersion7();

        // Act
        Result<GetAccount.Response> result = await SendAsync<GetAccount.Query, GetAccount.Response>(
            new GetAccount.Query(nonExistentId)
        );

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Helpers

    #endregion
}
