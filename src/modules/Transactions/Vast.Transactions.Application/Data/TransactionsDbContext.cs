using Vast.Common.Application.Database;
using Vast.Transactions.Domain.Customer;
using Vast.Transactions.Domain.Transaction;
using Microsoft.EntityFrameworkCore;

namespace Vast.Transactions.Application.Data;

public class TransactionsDbContext(DbContextOptions<TransactionsDbContext> options)
    : DbContext(options)
{
    internal DbSet<Transaction> Transactions => Set<Transaction>();
    internal DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Transactions);
        modelBuilder.ApplyConfigurationsFromAssembly(Common.Application.AssemblyReference.Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
    }
}
