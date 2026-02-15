using Deep.Accounts.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Deep.Accounts.Application.Data;

internal sealed class AccountRepository(AccountsDbContext db) : IAccountRepository
{
    public async Task<Account?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.Accounts.SingleOrDefaultAsync(acct => acct.Id == id, cancellationToken);

    public void Insert(Account account)
    {
        foreach (Role role in account.Roles)
        {
            db.Attach(role);
        }

        db.Accounts.Add(account);
    }
}
