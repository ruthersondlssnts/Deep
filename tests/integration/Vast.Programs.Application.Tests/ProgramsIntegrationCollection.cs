namespace Vast.Programs.Application.Tests;

/// <summary>
/// xUnit collection definition for Programs integration tests.
/// </summary>
[CollectionDefinition(nameof(ProgramsIntegrationCollection))]
public sealed class ProgramsIntegrationCollection
    : ICollectionFixture<ProgramsWebApplicationFactory>;
