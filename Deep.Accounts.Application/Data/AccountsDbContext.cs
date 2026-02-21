using Deep.Accounts.Domain.Accounts;
using Deep.Common.Application.Database;
using Microsoft.EntityFrameworkCore;

namespace Deep.Accounts.Application.Data;

public class AccountsDbContext(DbContextOptions<AccountsDbContext> options) : DbContext(options)
{
    internal DbSet<Account> Accounts => Set<Account>();
    internal DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Accounts);
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
    }
}
