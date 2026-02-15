using Deep.Transactions.Domain.Transaction;
using Microsoft.EntityFrameworkCore;

namespace Deep.Transactions.Application.Data;

public class TransactionRepository(TransactionsDbContext db) : ITransactionRepository
{
    public async Task<Transaction?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) => await db.Transactions.SingleOrDefaultAsync(acct => acct.Id == id, cancellationToken);

    public void Insert(Transaction account) => db.Transactions.Add(account);
}
