using System.Reflection;
using Deep.Accounts.Domain.Accounts;
using Deep.Architecture.Tests.Abstractions;
using Deep.Transactions.Domain.Transaction;
using DomainProgram = Deep.Programs.Domain.Programs.Program;

namespace Deep.Architecture.Tests.Modules;

public sealed class ModuleTests : BaseTest
{
    [Fact]
    public void AccountsModule_ShouldNotHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules = [ProgramsNamespace, TransactionsNamespace];
        string[] integrationEventsModules =
        [
            ProgramsIntegrationEventsNamespace,
            TransactionsIntegrationEventsNamespace,
        ];

        List<Assembly> accountsAssemblies =
        [
            typeof(Account).Assembly,
            Accounts.Application.AssemblyReference.Assembly,
        ];

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
    public void ProgramsModule_ShouldNotHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules = [AccountsNamespace, TransactionsNamespace];
        string[] integrationEventsModules =
        [
            AccountsIntegrationEventsNamespace,
            TransactionsIntegrationEventsNamespace,
        ];

        List<Assembly> programsAssemblies =
        [
            typeof(DomainProgram).Assembly,
            Programs.Application.AssemblyReference.Assembly,
        ];

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
    public void TransactionsModule_ShouldNotHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules = [AccountsNamespace, ProgramsNamespace];
        string[] integrationEventsModules =
        [
            AccountsIntegrationEventsNamespace,
            ProgramsIntegrationEventsNamespace,
        ];

        List<Assembly> transactionsAssemblies =
        [
            typeof(Transaction).Assembly,
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
