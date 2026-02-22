namespace Deep.Accounts.Domain.Accounts;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default
    );
    void Insert(PasswordResetToken passwordResetToken);
    void InvalidateAllForAccount(Guid accountId);
}
