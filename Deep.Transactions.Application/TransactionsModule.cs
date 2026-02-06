// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Common.Application;
using Deep.Common.Application.Database;
using Deep.Transactions.Application.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Transactions.Application;

public static class TransactionsModule
{
    public static IServiceCollection AddTransactionsModule(this IServiceCollection services)
    {
        services.AddDomainEventHandlers(AssemblyReference.Assembly)
                .AddPostgresDbContextWithSchema<TransactionsDbContext>(Schemas.Transactions)
                .AddEndpoints(AssemblyReference.Assembly)
                .AddDomainEventInterceptor<TransactionsDbContext>(AssemblyReference.Assembly);
        return services;
    }

    public static void ConfigureConsumers(MassTransit.IRegistrationConfigurator registrationConfigurator) =>
        ModuleRegistrationHelper.ConfigureConsumers(AssemblyReference.Assembly, registrationConfigurator);
}
