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

[Collection(nameof(AccountsIntegrationCollection))]
public class AccountsIntegrationTests(AccountsWebApplicationFactory factory)
    : AccountsIntegrationTestBase(factory)
{
    #region Register Account

    [Fact]
    public async Task RegisterAccount_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        RegisterAccountCommand request = new(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            Faker.Internet.Email(),
            "Test1234!",
            [RoleNames.ItAdmin]
        );

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/accounts/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        RegisterAccountResponse? result = await response.Content.ReadFromJsonAsync<RegisterAccountResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
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
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/accounts/register", duplicateRequest);

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
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/accounts/register", request);

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
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/accounts/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion

    #region Login

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        string email = Faker.Internet.Email();
        string password = "Test1234!";
        await RegisterTestAccountAsync(email, password);

        LoginAccount.Command loginRequest = new(email, password);

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/accounts/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        LoginAccount.Response? result = await response.Content.ReadFromJsonAsync<LoginAccount.Response>();
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
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/accounts/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Refresh Token

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
        LoginAccount.Response? loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginAccount.Response>();

        RefreshAccessToken.Command refreshRequest = new(loginResult!.RefreshToken);

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/accounts/refresh-token", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RefreshAccessToken.Response? result = await response.Content.ReadFromJsonAsync<RefreshAccessToken.Response>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe(loginResult.RefreshToken);
    }

    #endregion

    #region Logout

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
        LoginAccount.Response? loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginAccount.Response>();

        LogoutAccount.Command logoutRequest = new(loginResult!.RefreshToken);

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/accounts/logout", logoutRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Forgot/Reset Password

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ShouldReturnResetToken()
    {
        // Arrange
        string email = Faker.Internet.Email();
        await RegisterTestAccountAsync(email, "Test1234!");

        ForgotPassword.Command request = new(email);

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/accounts/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ForgotPassword.Response? result = await response.Content.ReadFromJsonAsync<ForgotPassword.Response>();
        result.Should().NotBeNull();
        result!.ResetToken.Should().NotBeNullOrEmpty();
    }

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
        ForgotPassword.Response? forgotResult = await forgotResponse.Content.ReadFromJsonAsync<ForgotPassword.Response>();

        ResetPassword.Command resetRequest = new(forgotResult!.ResetToken, "NewPassword1234!");

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/accounts/reset-password", resetRequest);

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

    #region Get Accounts

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
        List<GetAccounts.Response>? accounts = await response.Content.ReadFromJsonAsync<List<GetAccounts.Response>>();
        accounts.Should().NotBeNull();
        accounts!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    #endregion

    #region Handler Tests (No Endpoint)

    [Fact]
    public async Task GetAccount_Handler_WithValidId_ShouldReturnAccount()
    {
        // Arrange
        string email = Faker.Internet.Email();
        HttpResponseMessage registerResponse = await HttpClient.PostAsJsonAsync(
            "/accounts/register",
            new RegisterAccountCommand(
                Faker.Name.FirstName(),
                Faker.Name.LastName(),
                email,
                "Test1234!",
                [RoleNames.ItAdmin]
            )
        );
        RegisterAccountResponse? registered = await registerResponse.Content.ReadFromJsonAsync<RegisterAccountResponse>();

        // Act
        Result<GetAccount.Response> result = await SendAsync<GetAccount.Query, GetAccount.Response>(
            new GetAccount.Query(registered!.Id)
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(email);
    }

    #endregion

    #region Helpers

    private async Task<RegisterAccountResponse> RegisterTestAccountAsync(string email, string password)
    {
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/accounts/register",
            new RegisterAccountCommand(
                Faker.Name.FirstName(),
                Faker.Name.LastName(),
                email,
                password,
                [RoleNames.ItAdmin]
            )
        );
        return (await response.Content.ReadFromJsonAsync<RegisterAccountResponse>())!;
    }

    #endregion
}
