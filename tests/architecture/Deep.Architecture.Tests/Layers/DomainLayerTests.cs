namespace Deep.Architecture.Tests.Layers;

public sealed class DomainLayerTests
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
        "Microsoft.AspNetCore"
    ];

    [Fact]
    public void AccountsDomain_ShouldNotHaveForbiddenDependencies()
    {
        // Act
        TestResult result = Types
            .InAssembly(AssemblyReferences.AccountsDomain)
            .ShouldNot()
            .HaveDependencyOnAny(ForbiddenNamespaces)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            GetFailureMessage("Deep.Accounts.Domain", result));
    }

    [Fact]
    public void ProgramsDomain_ShouldNotHaveForbiddenDependencies()
    {
        // Act
        TestResult result = Types
            .InAssembly(AssemblyReferences.ProgramsDomain)
            .ShouldNot()
            .HaveDependencyOnAny(ForbiddenNamespaces)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            GetFailureMessage("Deep.Programs.Domain", result));
    }

    [Fact]
    public void TransactionsDomain_ShouldNotHaveForbiddenDependencies()
    {
        // Act
        TestResult result = Types
            .InAssembly(AssemblyReferences.TransactionsDomain)
            .ShouldNot()
            .HaveDependencyOnAny(ForbiddenNamespaces)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            GetFailureMessage("Deep.Transactions.Domain", result));
    }

    [Fact]
    public void CommonDomain_ShouldNotHaveForbiddenDependencies()
    {
        // Act
        TestResult result = Types
            .InAssembly(AssemblyReferences.CommonDomain)
            .ShouldNot()
            .HaveDependencyOnAny(ForbiddenNamespaces)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            GetFailureMessage("Deep.Common.Domain", result));
    }

    [Theory]
    [InlineData("Deep.Accounts.Domain")]
    [InlineData("Deep.Programs.Domain")]
    [InlineData("Deep.Transactions.Domain")]
    [InlineData("Deep.Common.Domain")]
    public void Domain_ShouldNotDependOnEntityFramework(string domainNamespace)
    {
        // Arrange
        var assembly = domainNamespace switch
        {
            "Deep.Accounts.Domain" => AssemblyReferences.AccountsDomain,
            "Deep.Programs.Domain" => AssemblyReferences.ProgramsDomain,
            "Deep.Transactions.Domain" => AssemblyReferences.TransactionsDomain,
            "Deep.Common.Domain" => AssemblyReferences.CommonDomain,
            _ => throw new ArgumentException($"Unknown domain: {domainNamespace}")
        };

        // Act
        TestResult result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"{domainNamespace} should not depend on Microsoft.EntityFrameworkCore. " +
            $"Violating types: {GetViolatingTypes(result)}");
    }

    [Theory]
    [InlineData("Deep.Accounts.Domain")]
    [InlineData("Deep.Programs.Domain")]
    [InlineData("Deep.Transactions.Domain")]
    [InlineData("Deep.Common.Domain")]
    public void Domain_ShouldNotDependOnAspNetCore(string domainNamespace)
    {
        // Arrange
        var assembly = domainNamespace switch
        {
            "Deep.Accounts.Domain" => AssemblyReferences.AccountsDomain,
            "Deep.Programs.Domain" => AssemblyReferences.ProgramsDomain,
            "Deep.Transactions.Domain" => AssemblyReferences.TransactionsDomain,
            "Deep.Common.Domain" => AssemblyReferences.CommonDomain,
            _ => throw new ArgumentException($"Unknown domain: {domainNamespace}")
        };

        // Act
        TestResult result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"{domainNamespace} should not depend on Microsoft.AspNetCore. " +
            $"Violating types: {GetViolatingTypes(result)}");
    }

    private static string GetFailureMessage(string layer, TestResult result)
    {
        return $"{layer} has forbidden dependencies. Violating types: {GetViolatingTypes(result)}";
    }

    private static string GetViolatingTypes(TestResult result)
    {
        return result.FailingTypes is null
            ? "None"
            : string.Join(", ", result.FailingTypes.Select(t => t.FullName));
    }
}
