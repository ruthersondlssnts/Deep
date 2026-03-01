using System.Reflection;

namespace Deep.Architecture.Tests;

public static class AssemblyReferences
{
    // Common
    public static readonly Assembly CommonDomain = typeof(Common.Domain.Entity).Assembly;
    public static readonly Assembly CommonApplication =
        typeof(Common.Application.IntegrationEvents.IntegrationEvent).Assembly;

    // Accounts Module
    public static readonly Assembly AccountsDomain =
        typeof(Accounts.Domain.Accounts.Account).Assembly;
    public static readonly Assembly AccountsApplication =
        typeof(Accounts.Application.AccountsModule).Assembly;
    public static readonly Assembly AccountsIntegrationEvents =
        typeof(Accounts.IntegrationEvents.AccountRegisteredIntegrationEvent).Assembly;

    // Programs Module
    public static readonly Assembly ProgramsDomain =
        typeof(Programs.Domain.Programs.Program).Assembly;
    public static readonly Assembly ProgramsApplication =
        typeof(Programs.Application.ProgramsModule).Assembly;
    public static readonly Assembly ProgramsIntegrationEvents =
        typeof(Programs.IntegrationEvents.UserRegisteredIntegrationEvent).Assembly;

    // Transactions Module
    public static readonly Assembly TransactionsDomain =
        typeof(Transactions.Domain.Transaction.Transaction).Assembly;
    public static readonly Assembly TransactionsApplication =
        typeof(Transactions.Application.TransactionsModule).Assembly;
    public static readonly Assembly TransactionsIntegrationEvents =
        typeof(Transactions.IntegrationEvents.TransactionCreatedIntegrationEvent).Assembly;

    // All Domain Assemblies
    public static readonly Assembly[] AllDomainAssemblies =
    [
        AccountsDomain,
        ProgramsDomain,
        TransactionsDomain,
    ];

    // All Application Assemblies
    public static readonly Assembly[] AllApplicationAssemblies =
    [
        AccountsApplication,
        ProgramsApplication,
        TransactionsApplication,
    ];

    // All IntegrationEvents Assemblies
    public static readonly Assembly[] AllIntegrationEventsAssemblies =
    [
        AccountsIntegrationEvents,
        ProgramsIntegrationEvents,
        TransactionsIntegrationEvents,
    ];
}
