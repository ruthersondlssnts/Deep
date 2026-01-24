using Deep.Common.Database;
using Microsoft.EntityFrameworkCore;
using Deep.Accounts.Domain.Accounts;

namespace Deep.Accounts.Application.Data;

public class AccountsDbContext(DbContextOptions<AccountsDbContext> options)
    : DbContext(options)
{
    internal DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Accounts);
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
    }
}
