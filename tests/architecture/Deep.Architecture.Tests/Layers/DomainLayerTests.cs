using Deep.Architecture.Tests.Abstractions;

namespace Deep.Architecture.Tests.Layers;

public sealed class DomainLayerTests : BaseTest
{
    private static readonly string[] ForbiddenNamespaces =
    [
        "Deep.Accounts.Application",
        "Deep.Programs.Application",
        "Deep.Transactions.Application",
        "Deep.Accounts.IntegrationEvents",
        "Deep.Programs.IntegrationEvents",
        "Deep.Transactions.IntegrationEvents",
        "Deep.Api",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
    ];

    [Fact]
    public void DomainLayer_ShouldNotHaveForbiddenDependencies()
    {
        Types
            .InAssemblies(AssemblyReferences.AllDomainAssemblies)
            .ShouldNot()
            .HaveDependencyOnAny(ForbiddenNamespaces)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void DomainLayer_ShouldNotDependOnEntityFramework()
    {
        Types
            .InAssemblies(AssemblyReferences.AllDomainAssemblies)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void DomainLayer_ShouldNotDependOnAspNetCore()
    {
        Types
            .InAssemblies(AssemblyReferences.AllDomainAssemblies)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult()
            .ShouldBeSuccessful();
    }
}
