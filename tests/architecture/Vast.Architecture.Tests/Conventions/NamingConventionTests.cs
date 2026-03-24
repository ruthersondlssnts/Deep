using Vast.Architecture.Tests.Abstractions;

namespace Vast.Architecture.Tests.Conventions;

public sealed class NamingConventionTests : BaseTest
{
    [Fact]
    public void Commands_ShouldBeSealed() =>
        Types
            .InAssemblies(AssemblyReferences.AllApplicationAssemblies)
            .That()
            .HaveNameEndingWith("Command", StringComparison.Ordinal)
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();

    [Fact]
    public void Queries_ShouldBeSealed() =>
        Types
            .InAssemblies(AssemblyReferences.AllApplicationAssemblies)
            .That()
            .HaveNameEndingWith("Query", StringComparison.Ordinal)
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();

    [Fact]
    public void Handlers_ShouldBeSealed() =>
        Types
            .InAssemblies(AssemblyReferences.AllApplicationAssemblies)
            .That()
            .HaveNameEndingWith("Handler", StringComparison.Ordinal)
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();

    [Fact]
    public void DomainEvents_ShouldBeSealed() =>
        Types
            .InAssemblies(AssemblyReferences.AllDomainAssemblies)
            .That()
            .Inherit(typeof(Common.Domain.DomainEvent))
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();

    [Fact]
    public void DomainEvents_ShouldEndWithDomainEvent() =>
        Types
            .InAssemblies(AssemblyReferences.AllDomainAssemblies)
            .That()
            .Inherit(typeof(Common.Domain.DomainEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent", StringComparison.Ordinal)
            .GetResult()
            .ShouldBeSuccessful();

    [Fact]
    public void IntegrationEvents_ShouldBeSealed() =>
        Types
            .InAssemblies(AssemblyReferences.AllIntegrationEventsAssemblies)
            .That()
            .HaveNameEndingWith("IntegrationEvent", StringComparison.Ordinal)
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();

    [Fact]
    public void IntegrationEvents_ShouldEndWithIntegrationEvent() =>
        Types
            .InAssemblies(AssemblyReferences.AllIntegrationEventsAssemblies)
            .That()
            .AreClasses()
            .And()
            .ArePublic()
            .Should()
            .HaveNameEndingWith("IntegrationEvent", StringComparison.Ordinal)
            .GetResult()
            .ShouldBeSuccessful();
}
