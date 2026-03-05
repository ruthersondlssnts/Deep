using System.Reflection;
using Deep.Accounts.Domain.Accounts;
using Deep.Transactions.Domain.Transaction;
using DomainProgram = Deep.Programs.Domain.Programs.Program;

namespace Deep.Architecture.Tests;

public static class AssemblyReferences
{
    // Common
    public static readonly Assembly CommonDomain = typeof(Common.Domain.Entity).Assembly;
    public static readonly Assembly CommonApplication =
        typeof(Common.Application.IntegrationEvents.IntegrationEvent).Assembly;

    // Accounts Module
    public static readonly Assembly AccountsDomain = typeof(Account).Assembly;
    public static readonly Assembly AccountsApplication =
        typeof(Accounts.Application.AssemblyReference).Assembly;
    public static readonly Assembly AccountsIntegrationEvents =
        typeof(Accounts.IntegrationEvents.AccountRegisteredIntegrationEvent).Assembly;

    // Programs Module
    public static readonly Assembly ProgramsDomain = typeof(DomainProgram).Assembly;
    public static readonly Assembly ProgramsApplication =
        typeof(Programs.Application.AssemblyReference).Assembly;
    public static readonly Assembly ProgramsIntegrationEvents =
        typeof(Programs.IntegrationEvents.UserRegisteredIntegrationEvent).Assembly;

    // Transactions Module
    public static readonly Assembly TransactionsDomain = typeof(Transaction).Assembly;
    public static readonly Assembly TransactionsApplication =
        typeof(Transactions.Application.AssemblyReference).Assembly;
    public static readonly Assembly TransactionsIntegrationEvents =
        typeof(Transactions.IntegrationEvents.TransactionCreatedIntegrationEvent).Assembly;

    // All Domain Assemblies (excluding CommonDomain for module-specific tests)
    public static readonly Assembly[] AllDomainAssemblies =
        [AccountsDomain, ProgramsDomain, TransactionsDomain];

    // All Application Assemblies
    public static readonly Assembly[] AllApplicationAssemblies =
        [AccountsApplication, ProgramsApplication, TransactionsApplication];

    // All IntegrationEvents Assemblies
    public static readonly Assembly[] AllIntegrationEventsAssemblies =
        [AccountsIntegrationEvents, ProgramsIntegrationEvents, TransactionsIntegrationEvents];
}
