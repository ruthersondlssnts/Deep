using Deep.Testing.Integration;

namespace Deep.Programs.Application.Tests;

/// <summary>
/// xUnit collection definition for Programs integration tests.
/// </summary>
[CollectionDefinition(nameof(ProgramsIntegrationCollection))]
public sealed class ProgramsIntegrationCollection : ICollectionFixture<DeepWebApplicationFactory>;
