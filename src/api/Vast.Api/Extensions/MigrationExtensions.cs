using Vast.Accounts.Application.Data;
using Vast.Programs.Application.Data;
using Vast.Transactions.Application.Data;
using Microsoft.EntityFrameworkCore;

namespace Vast.Api.Extensions;

internal static class MigrationExtensions
{
    internal static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        ApplyMigrations<ProgramsDbContext>(scope);
        ApplyMigrations<AccountsDbContext>(scope);
        ApplyMigrations<TransactionsDbContext>(scope);
    }

    private static void ApplyMigrations<TDbContext>(IServiceScope scope)
        where TDbContext : DbContext
    {
        using TDbContext context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        context.Database.Migrate();
    }
}
