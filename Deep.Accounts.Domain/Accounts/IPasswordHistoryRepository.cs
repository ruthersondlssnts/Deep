namespace Deep.Accounts.Domain.Accounts;

public interface IPasswordHistoryRepository
{
    Task<IReadOnlyCollection<PasswordHistory>> GetLastNByAccountIdAsync(
        Guid accountId,
        int count,
        CancellationToken cancellationToken = default
    );
    void Insert(PasswordHistory passwordHistory);
    void DeleteOldestBeyondLimit(Guid accountId, int keepCount);
}
