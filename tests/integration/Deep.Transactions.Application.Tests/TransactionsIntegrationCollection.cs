namespace Deep.Transactions.Application.Tests;

[CollectionDefinition(nameof(TransactionsIntegrationCollection))]
public sealed class TransactionsIntegrationCollection
    : ICollectionFixture<TransactionsWebApplicationFactory>;
