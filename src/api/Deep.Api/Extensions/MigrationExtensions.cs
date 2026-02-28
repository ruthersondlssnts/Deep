using Deep.Accounts.Application.Data;
using Deep.Programs.Application.Data;
using Deep.Transactions.Application.Data;
using Microsoft.EntityFrameworkCore;

namespace Deep.Api.Extensions;

public static class MigrationExtensions
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
