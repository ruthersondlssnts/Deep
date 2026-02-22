using Deep.Accounts.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Deep.Accounts.Application.Data;

internal sealed class RefreshTokenRepository(AccountsDbContext db) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) =>
        await db.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == token, cancellationToken);

    public async Task<IReadOnlyCollection<RefreshToken>> GetActiveByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default) =>
        await db.RefreshTokens
            .Where(rt => rt.AccountId == accountId && rt.RevokedAtUtc == null && rt.ExpiryDateUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

    public void Insert(RefreshToken refreshToken) =>
        db.RefreshTokens.Add(refreshToken);

    public void RevokeAllForAccount(Guid accountId)
    {
        var activeTokens = db.RefreshTokens
            .Where(rt => rt.AccountId == accountId && rt.RevokedAtUtc == null)
            .ToList();

        foreach (var token in activeTokens)
        {
            token.Revoke();
        }
    }
}
