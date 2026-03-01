namespace Deep.Architecture.Tests.Conventions;

public sealed class NamingConventionTests
{
    #region Commands

    [Fact]
    public void Commands_ShouldBeSealed()
    {
        // Act
        TestResult result = Types
            .InAssemblies(AssemblyReferences.AllApplicationAssemblies)
            .That()
            .HaveNameEndingWith("Command")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue($"All Commands should be sealed. Violating types: {GetViolatingTypes(result)}");
    }

    [Fact]
    public void CommandTypes_ShouldEndWithCommand()
    {
        // Act - Verify classes with Command suffix follow naming convention
        TestResult result = Types
            .InAssemblies(AssemblyReferences.AllApplicationAssemblies)
            .That()
            .HaveNameEndingWith("Command")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Command")
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue(
                $"All Command types should end with 'Command'. Violating types: {GetViolatingTypes(result)}"
            );
    }

    #endregion

    #region Queries

    [Fact]
    public void Queries_ShouldBeSealed()
    {
        // Act
        TestResult result = Types
            .InAssemblies(AssemblyReferences.AllApplicationAssemblies)
            .That()
            .HaveNameEndingWith("Query")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue($"All Queries should be sealed. Violating types: {GetViolatingTypes(result)}");
    }

    [Fact]
    public void QueryTypes_ShouldEndWithQuery()
    {
        // Act - Verify classes with Query suffix follow naming convention
        TestResult result = Types
            .InAssemblies(AssemblyReferences.AllApplicationAssemblies)
            .That()
            .HaveNameEndingWith("Query")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Query")
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue(
                $"All Query types should end with 'Query'. Violating types: {GetViolatingTypes(result)}"
            );
    }

    #endregion

    #region Handlers

    [Fact]
    public void Handlers_ShouldBeSealed()
    {
        // Act
        TestResult result = Types
            .InAssemblies(AssemblyReferences.AllApplicationAssemblies)
            .That()
            .HaveNameEndingWith("Handler")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue($"All Handlers should be sealed. Violating types: {GetViolatingTypes(result)}");
    }

    [Fact]
    public void Handlers_ShouldBeInternal()
    {
        // Act
        TestResult result = Types
            .InAssemblies(AssemblyReferences.AllApplicationAssemblies)
            .That()
            .HaveNameEndingWith("Handler")
            .And()
            .AreClasses()
            .Should()
            .NotBePublic()
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue(
                $"All Handlers should be internal (not public). Violating types: {GetViolatingTypes(result)}"
            );
    }

    #endregion

    #region Domain Events

    [Fact]
    public void DomainEvents_ShouldBeSealed()
    {
        // Act
        TestResult result = Types
            .InAssemblies(AssemblyReferences.AllDomainAssemblies)
            .That()
            .HaveNameEndingWith("DomainEvent")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue(
                $"All DomainEvents should be sealed. Violating types: {GetViolatingTypes(result)}"
            );
    }

    [Fact]
    public void DomainEvents_ShouldEndWithDomainEvent()
    {
        // Act
        TestResult result = Types
            .InAssemblies(AssemblyReferences.AllDomainAssemblies)
            .That()
            .Inherit(typeof(Common.Domain.DomainEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue(
                $"All types inheriting from DomainEvent should end with 'DomainEvent'. Violating types: {GetViolatingTypes(result)}"
            );
    }

    #endregion

    #region Integration Events

    [Fact]
    public void IntegrationEvents_ShouldBeSealed()
    {
        // Act
        TestResult result = Types
            .InAssemblies(AssemblyReferences.AllIntegrationEventsAssemblies)
            .That()
            .HaveNameEndingWith("IntegrationEvent")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue(
                $"All IntegrationEvents should be sealed. Violating types: {GetViolatingTypes(result)}"
            );
    }

    [Fact]
    public void IntegrationEvents_ShouldEndWithIntegrationEvent()
    {
        // Act
        TestResult result = Types
            .InAssemblies(AssemblyReferences.AllIntegrationEventsAssemblies)
            .That()
            .AreClasses()
            .And()
            .ArePublic()
            .Should()
            .HaveNameEndingWith("IntegrationEvent")
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue(
                $"All public classes in IntegrationEvents projects should end with 'IntegrationEvent'. Violating types: {GetViolatingTypes(result)}"
            );
    }

    #endregion

    private static string GetViolatingTypes(TestResult result) =>
        result.FailingTypes is null
            ? "None"
            : string.Join(", ", result.FailingTypes.Select(t => t.FullName));
}
