using Vast.Architecture.Tests.Abstractions;

namespace Vast.Architecture.Tests.Layers;

public sealed class DomainLayerTests : BaseTest
{
    private static readonly string[] ForbiddenNamespaces =
    [
        "Vast.Accounts.Application",
        "Vast.Programs.Application",
        "Vast.Transactions.Application",
        "Vast.Accounts.IntegrationEvents",
        "Vast.Programs.IntegrationEvents",
        "Vast.Transactions.IntegrationEvents",
        "Vast.Api",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
    ];

    [Fact]
    public void DomainLayer_ShouldNotHaveForbiddenDependencies() =>
        Types
            .InAssemblies(AssemblyReferences.AllDomainAssemblies)
            .ShouldNot()
            .HaveDependencyOnAny(ForbiddenNamespaces)
            .GetResult()
            .ShouldBeSuccessful();

    [Fact]
    public void DomainLayer_ShouldNotDependOnEntityFramework() =>
        Types
            .InAssemblies(AssemblyReferences.AllDomainAssemblies)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult()
            .ShouldBeSuccessful();

    [Fact]
    public void DomainLayer_ShouldNotDependOnAspNetCore() =>
        Types
            .InAssemblies(AssemblyReferences.AllDomainAssemblies)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult()
            .ShouldBeSuccessful();
}
