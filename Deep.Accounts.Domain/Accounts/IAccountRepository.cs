namespace Deep.Accounts.Domain.Accounts;

public interface IAccountRepository
{
    Task<Account?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    void Insert(Account account);
}
