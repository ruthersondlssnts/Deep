namespace Vast.Accounts.Application.Tests;

/// <summary>
/// xUnit collection definition for Accounts integration tests.
/// </summary>
[CollectionDefinition(nameof(AccountsIntegrationCollection))]
public sealed class AccountsIntegrationCollection
    : ICollectionFixture<AccountsWebApplicationFactory>;
