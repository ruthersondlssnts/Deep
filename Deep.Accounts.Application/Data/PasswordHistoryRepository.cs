using Deep.Accounts.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Deep.Accounts.Application.Data;

internal sealed class PasswordHistoryRepository(AccountsDbContext db) : IPasswordHistoryRepository
{
    public async Task<IReadOnlyCollection<PasswordHistory>> GetLastNByAccountIdAsync(
        Guid accountId,
        int count,
        CancellationToken cancellationToken = default
    ) =>
        await db
            .PasswordHistories.Where(ph => ph.AccountId == accountId)
            .OrderByDescending(ph => ph.ChangedAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);

    public void Insert(PasswordHistory passwordHistory) =>
        db.PasswordHistories.Add(passwordHistory);

    public void DeleteOldestBeyondLimit(Guid accountId, int keepCount)
    {
        var toDelete = db
            .PasswordHistories.Where(ph => ph.AccountId == accountId)
            .OrderByDescending(ph => ph.ChangedAtUtc)
            .Skip(keepCount)
            .ToList();

        db.PasswordHistories.RemoveRange(toDelete);
    }
}
