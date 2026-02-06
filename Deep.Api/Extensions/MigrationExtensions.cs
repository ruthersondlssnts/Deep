// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Accounts.Application.Data;
using Deep.Programs.Application.Data;
using Deep.Transactions.Application.Data;
using Microsoft.EntityFrameworkCore;

namespace Deep.Api.Extensions
{
    public static class MigrationExtensions
    {
        internal static void ApplyMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            ApplyMigrations<ProgramsDbContext>(scope);
            ApplyMigrations<AccountsDbContext>(scope);
            ApplyMigrations<TransactionsDbContext>(scope);
        }

        private static void ApplyMigrations<TDbContext>(IServiceScope scope)
            where TDbContext : DbContext
        {
            using var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

            context.Database.Migrate();
        }
    }
}
