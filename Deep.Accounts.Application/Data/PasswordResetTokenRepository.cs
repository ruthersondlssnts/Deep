using Deep.Accounts.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Deep.Accounts.Application.Data;

internal sealed class PasswordResetTokenRepository(AccountsDbContext db) : IPasswordResetTokenRepository
{
    public async Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) =>
        await db.PasswordResetTokens.SingleOrDefaultAsync(prt => prt.Token == token, cancellationToken);

    public void Insert(PasswordResetToken passwordResetToken) =>
        db.PasswordResetTokens.Add(passwordResetToken);

    public void InvalidateAllForAccount(Guid accountId)
    {
        var activeTokens = db.PasswordResetTokens
            .Where(prt => prt.AccountId == accountId && prt.UsedAtUtc == null)
            .ToList();

        foreach (var token in activeTokens)
        {
            token.MarkAsUsed();
        }
    }
}
