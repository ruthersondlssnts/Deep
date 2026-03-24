using System.Reflection;
using Vast.Architecture.Tests.Abstractions;

namespace Vast.Architecture.Tests.Layers;

public sealed class ApplicationLayerTests : BaseTest
{
    [Fact]
    public void AccountsApplication_ShouldNotDependOnOtherModules()
    {
        string[] otherModules = [ProgramsNamespace, TransactionsNamespace];
        string[] integrationEventsModules =
        [
            ProgramsIntegrationEventsNamespace,
            TransactionsIntegrationEventsNamespace,
        ];

        List<Assembly> accountsAssemblies = [Accounts.Application.AssemblyReference.Assembly];

        Types
            .InAssemblies(accountsAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void ProgramsApplication_ShouldNotDependOnOtherModules()
    {
        string[] otherModules = [AccountsNamespace, TransactionsNamespace];
        string[] integrationEventsModules =
        [
            AccountsIntegrationEventsNamespace,
            TransactionsIntegrationEventsNamespace,
        ];

        List<Assembly> programsAssemblies = [Programs.Application.AssemblyReference.Assembly];

        Types
            .InAssemblies(programsAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void TransactionsApplication_ShouldNotDependOnOtherModules()
    {
        string[] otherModules = [AccountsNamespace, ProgramsNamespace];
        string[] integrationEventsModules =
        [
            AccountsIntegrationEventsNamespace,
            ProgramsIntegrationEventsNamespace,
        ];

        List<Assembly> transactionsAssemblies =
        [
            Transactions.Application.AssemblyReference.Assembly,
        ];

        Types
            .InAssemblies(transactionsAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }
}
