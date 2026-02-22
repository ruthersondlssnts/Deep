namespace Deep.Accounts.Domain.Accounts;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyCollection<RefreshToken>> GetActiveByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default
    );
    void Insert(RefreshToken refreshToken);
    void RevokeAllForAccount(Guid accountId);
}
