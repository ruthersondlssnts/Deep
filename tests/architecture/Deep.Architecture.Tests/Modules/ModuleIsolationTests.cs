namespace Deep.Architecture.Tests.Modules;

public sealed class ModuleIsolationTests
{
    // Note: IntegrationEvents are the PUBLIC API for cross-module communication.
    // Application layers MAY depend on IntegrationEvents from other modules.
    // Domain and Application internals must NOT be accessed cross-module.

    #region Accounts Module Isolation

    [Fact]
    public void AccountsDomain_ShouldNotDependOnOtherModules()
    {
        // Arrange - Domain should not depend on ANY other module (including IntegrationEvents)
        string[] forbiddenNamespaces =
        [
            "Deep.Programs.Domain",
            "Deep.Programs.Application",
            "Deep.Programs.IntegrationEvents",
            "Deep.Transactions.Domain",
            "Deep.Transactions.Application",
            "Deep.Transactions.IntegrationEvents",
        ];

        // Act
        TestResult result = Types
            .InAssembly(AssemblyReferences.AccountsDomain)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue(GetFailureMessage("Accounts.Domain", "other modules", result));
    }

    [Fact]
    public void AccountsApplication_ShouldNotDependOnOtherModuleInternals()
    {
        // Arrange - Application may depend on IntegrationEvents but NOT on Domain/Application internals
        string[] forbiddenNamespaces =
        [
            "Deep.Programs.Domain",
            "Deep.Programs.Application",
            "Deep.Transactions.Domain",
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
            .BeTrue(GetFailureMessage("Accounts.Application", "other module internals", result));
    }

    #endregion

    #region Programs Module Isolation

    [Fact]
    public void ProgramsDomain_ShouldNotDependOnOtherModules()
    {
        // Arrange - Domain should not depend on ANY other module
        string[] forbiddenNamespaces =
        [
            "Deep.Accounts.Domain",
            "Deep.Accounts.Application",
            "Deep.Accounts.IntegrationEvents",
            "Deep.Transactions.Domain",
            "Deep.Transactions.Application",
            "Deep.Transactions.IntegrationEvents",
        ];

        // Act
        TestResult result = Types
            .InAssembly(AssemblyReferences.ProgramsDomain)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue(GetFailureMessage("Programs.Domain", "other modules", result));
    }

    [Fact]
    public void ProgramsApplication_ShouldNotDependOnOtherModuleInternals()
    {
        // Arrange - Application may depend on IntegrationEvents but NOT on Domain/Application internals
        string[] forbiddenNamespaces =
        [
            "Deep.Accounts.Domain",
            "Deep.Accounts.Application",
            "Deep.Transactions.Domain",
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
            .BeTrue(GetFailureMessage("Programs.Application", "other module internals", result));
    }

    #endregion

    #region Transactions Module Isolation

    [Fact]
    public void TransactionsDomain_ShouldNotDependOnOtherModules()
    {
        // Arrange - Domain should not depend on ANY other module
        string[] forbiddenNamespaces =
        [
            "Deep.Accounts.Domain",
            "Deep.Accounts.Application",
            "Deep.Accounts.IntegrationEvents",
            "Deep.Programs.Domain",
            "Deep.Programs.Application",
            "Deep.Programs.IntegrationEvents",
        ];

        // Act
        TestResult result = Types
            .InAssembly(AssemblyReferences.TransactionsDomain)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        // Assert
        result
            .IsSuccessful.Should()
            .BeTrue(GetFailureMessage("Transactions.Domain", "other modules", result));
    }

    [Fact]
    public void TransactionsApplication_ShouldNotDependOnOtherModuleInternals()
    {
        // Arrange - Application may depend on IntegrationEvents but NOT on Domain/Application internals
        string[] forbiddenNamespaces =
        [
            "Deep.Accounts.Domain",
            "Deep.Accounts.Application",
            "Deep.Programs.Domain",
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
                GetFailureMessage("Transactions.Application", "other module internals", result)
            );
    }

    #endregion

    #region Integration Events Cross-Module Communication

    [Fact]
    public void IntegrationEvents_ShouldNotDependOnModuleInternals()
    {
        // Integration events should be standalone and not depend on module internals
        string[] allModuleInternals =
        [
            "Deep.Accounts.Domain",
            "Deep.Accounts.Application",
            "Deep.Programs.Domain",
            "Deep.Programs.Application",
            "Deep.Transactions.Domain",
            "Deep.Transactions.Application",
        ];

        // Act
        TestResult accountsEventsResult = Types
            .InAssembly(AssemblyReferences.AccountsIntegrationEvents)
            .ShouldNot()
            .HaveDependencyOnAny(allModuleInternals)
            .GetResult();

        TestResult programsEventsResult = Types
            .InAssembly(AssemblyReferences.ProgramsIntegrationEvents)
            .ShouldNot()
            .HaveDependencyOnAny(allModuleInternals)
            .GetResult();

        TestResult transactionsEventsResult = Types
            .InAssembly(AssemblyReferences.TransactionsIntegrationEvents)
            .ShouldNot()
            .HaveDependencyOnAny(allModuleInternals)
            .GetResult();

        // Assert
        accountsEventsResult
            .IsSuccessful.Should()
            .BeTrue(
                $"Accounts.IntegrationEvents should not depend on module internals. Violating: {GetViolatingTypes(accountsEventsResult)}"
            );
        programsEventsResult
            .IsSuccessful.Should()
            .BeTrue(
                $"Programs.IntegrationEvents should not depend on module internals. Violating: {GetViolatingTypes(programsEventsResult)}"
            );
        transactionsEventsResult
            .IsSuccessful.Should()
            .BeTrue(
                $"Transactions.IntegrationEvents should not depend on module internals. Violating: {GetViolatingTypes(transactionsEventsResult)}"
            );
    }

    #endregion

    private static string GetFailureMessage(
        string sourceModule,
        string targetModule,
        TestResult result
    ) =>
        $"{sourceModule} should not depend on {targetModule} module. "
        + $"Violating types: {GetViolatingTypes(result)}";

    private static string GetViolatingTypes(TestResult result) =>
        result.FailingTypes is null
            ? "None"
            : string.Join(", ", result.FailingTypes.Select(t => t.FullName));
}
