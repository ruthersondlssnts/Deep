using Deep.Architecture.Tests.Abstractions;

namespace Deep.Architecture.Tests.Conventions;

public sealed class NamingConventionTests : BaseTest
{
    [Fact]
    public void Commands_ShouldBeSealed()
    {
        Types
            .InAssemblies(AssemblyReferences.AllApplicationAssemblies)
            .That()
            .HaveNameEndingWith("Command")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Queries_ShouldBeSealed()
    {
        Types
            .InAssemblies(AssemblyReferences.AllApplicationAssemblies)
            .That()
            .HaveNameEndingWith("Query")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Handlers_ShouldBeSealed()
    {
        Types
            .InAssemblies(AssemblyReferences.AllApplicationAssemblies)
            .That()
            .HaveNameEndingWith("Handler")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void DomainEvents_ShouldBeSealed()
    {
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
    }

    [Fact]
    public void DomainEvents_ShouldEndWithDomainEvent()
    {
        Types
            .InAssemblies(AssemblyReferences.AllDomainAssemblies)
            .That()
            .Inherit(typeof(Common.Domain.DomainEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void IntegrationEvents_ShouldBeSealed()
    {
        Types
            .InAssemblies(AssemblyReferences.AllIntegrationEventsAssemblies)
            .That()
            .HaveNameEndingWith("IntegrationEvent")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void IntegrationEvents_ShouldEndWithIntegrationEvent()
    {
        Types
            .InAssemblies(AssemblyReferences.AllIntegrationEventsAssemblies)
            .That()
            .AreClasses()
            .And()
            .ArePublic()
            .Should()
            .HaveNameEndingWith("IntegrationEvent")
            .GetResult()
            .ShouldBeSuccessful();
    }
}
