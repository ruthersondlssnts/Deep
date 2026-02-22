using Deep.Accounts.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Deep.Accounts.Application.Data;

internal sealed class AccountRepository(AccountsDbContext db) : IAccountRepository
{
    public async Task<Account?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db
            .Accounts.Include(a => a.Roles)
            .SingleOrDefaultAsync(acct => acct.Id == id, cancellationToken);

    public async Task<Account?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    ) =>
        await db
            .Accounts.Include(a => a.Roles)
            .SingleOrDefaultAsync(acct => acct.Email == email, cancellationToken);

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    ) => await db.Accounts.AnyAsync(a => a.Email == email, cancellationToken);

    public void Insert(Account account)
    {
        foreach (Role role in account.Roles)
        {
            db.Attach(role);
        }

        db.Accounts.Add(account);
    }
}
