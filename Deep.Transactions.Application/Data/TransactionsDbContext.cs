using Deep.Common.Application.Database;
using Deep.Transactions.Domain.Customer;
using Deep.Transactions.Domain.Transaction;
using Microsoft.EntityFrameworkCore;

namespace Deep.Transactions.Application.Data;

public class TransactionsDbContext(DbContextOptions<TransactionsDbContext> options)
: DbContext(options)
{
    internal DbSet<Transaction> Transactions => Set<Transaction>();
    internal DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Transactions);
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
    }
}
