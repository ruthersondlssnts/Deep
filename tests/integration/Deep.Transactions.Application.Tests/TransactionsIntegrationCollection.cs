using Deep.Testing.Integration;

namespace Deep.Transactions.Application.Tests;

/// <summary>
/// xUnit collection definition for Transactions integration tests.
/// </summary>
[CollectionDefinition(nameof(TransactionsIntegrationCollection))]
public sealed class TransactionsIntegrationCollection : ICollectionFixture<DeepWebApplicationFactory>;
