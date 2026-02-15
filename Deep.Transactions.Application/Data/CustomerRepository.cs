using Deep.Transactions.Domain.Customer;
using Microsoft.EntityFrameworkCore;

namespace Deep.Transactions.Application.Data;

public class CustomerRepository(TransactionsDbContext db) : ICustomerRepository
{
    public async Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.Customers.SingleOrDefaultAsync(acct => acct.Id == id, cancellationToken);

    public void Insert(Customer account) => db.Customers.Add(account);
}
