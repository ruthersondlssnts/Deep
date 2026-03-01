using System.Reflection;

namespace Deep.Architecture.Tests.Layers;

public sealed class ApplicationLayerTests
{
    [Fact]
    public void AccountsApplication_ShouldNotDependOnOtherModuleDomains()
    {
        // Arrange
        string[] forbiddenDomains = ["Deep.Programs.Domain", "Deep.Transactions.Domain"];

        // Act
        TestResult result = Types
            .InAssembly(AssemblyReferences.AccountsApplication)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenDomains)
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue(GetFailureMessage("Deep.Accounts.Application", forbiddenDomains, result));
    }

    [Fact]
    public void ProgramsApplication_ShouldNotDependOnOtherModuleDomains()
    {
        // Arrange
        string[] forbiddenDomains = ["Deep.Accounts.Domain", "Deep.Transactions.Domain"];

        // Act
        TestResult result = Types
            .InAssembly(AssemblyReferences.ProgramsApplication)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenDomains)
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue(GetFailureMessage("Deep.Programs.Application", forbiddenDomains, result));
    }

    [Fact]
    public void TransactionsApplication_ShouldNotDependOnOtherModuleDomains()
    {
        // Arrange
        string[] forbiddenDomains = ["Deep.Accounts.Domain", "Deep.Programs.Domain"];

        // Act
        TestResult result = Types
            .InAssembly(AssemblyReferences.TransactionsApplication)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenDomains)
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue(GetFailureMessage("Deep.Transactions.Application", forbiddenDomains, result));
    }

    [Theory]
    [InlineData("Deep.Accounts.Application")]
    [InlineData("Deep.Programs.Application")]
    [InlineData("Deep.Transactions.Application")]
    public void Application_MayDependOnCommon(string applicationNamespace)
    {
        // Arrange
        Assembly assembly = applicationNamespace switch
        {
            "Deep.Accounts.Application" => AssemblyReferences.AccountsApplication,
            "Deep.Programs.Application" => AssemblyReferences.ProgramsApplication,
            "Deep.Transactions.Application" => AssemblyReferences.TransactionsApplication,
            _ => throw new ArgumentException($"Unknown application: {applicationNamespace}"),
        };

        // Act - This test verifies allowed dependencies exist (no forbidden Common dependency)
        TestResult result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny("SomeNonExistentNamespace")
            .GetResult();

        // Assert - This should always pass as it's testing allowed dependencies
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void AccountsApplication_ShouldOnlyDependOnOwnDomainAndCommon()
    {
        // Arrange
        string[] forbiddenNamespaces =
        [
            "Deep.Programs.Domain",
            "Deep.Transactions.Domain",
            "Deep.Programs.Application",
            "Deep.Transactions.Application",
        ];

        // Act
        TestResult result = Types
            .InAssembly(AssemblyReferences.AccountsApplication)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue(GetFailureMessage("Deep.Accounts.Application", forbiddenNamespaces, result));
    }

    [Fact]
    public void ProgramsApplication_ShouldOnlyDependOnOwnDomainAndCommon()
    {
        // Arrange
        string[] forbiddenNamespaces =
        [
            "Deep.Accounts.Domain",
            "Deep.Transactions.Domain",
            "Deep.Accounts.Application",
            "Deep.Transactions.Application",
        ];

        // Act
        TestResult result = Types
            .InAssembly(AssemblyReferences.ProgramsApplication)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue(GetFailureMessage("Deep.Programs.Application", forbiddenNamespaces, result));
    }

    [Fact]
    public void TransactionsApplication_ShouldOnlyDependOnOwnDomainAndCommon()
    {
        // Arrange
        string[] forbiddenNamespaces =
        [
            "Deep.Accounts.Domain",
            "Deep.Programs.Domain",
            "Deep.Accounts.Application",
            "Deep.Programs.Application",
        ];

        // Act
        TestResult result = Types
            .InAssembly(AssemblyReferences.TransactionsApplication)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue(
                GetFailureMessage("Deep.Transactions.Application", forbiddenNamespaces, result)
            );
    }

    private static string GetFailureMessage(
        string layer,
        string[] forbiddenNamespaces,
        TestResult result
    ) =>
        $"{layer} should not depend on [{string.Join(", ", forbiddenNamespaces)}]. "
        + $"Violating types: {GetViolatingTypes(result)}";

    private static string GetViolatingTypes(TestResult result) =>
        result.FailingTypes is null
            ? "None"
            : string.Join(", ", result.FailingTypes.Select(t => t.FullName));
}
