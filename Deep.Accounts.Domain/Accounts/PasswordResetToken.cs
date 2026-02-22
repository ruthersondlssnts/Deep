using System.Security.Cryptography;

namespace Deep.Accounts.Domain.Accounts;

public sealed class PasswordResetToken
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiryDateUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiryDateUtc;
    public bool IsUsed => UsedAtUtc is not null;
    public bool IsValid => !IsExpired && !IsUsed;

    private PasswordResetToken() { }

    public static PasswordResetToken Create(Guid accountId, TimeSpan lifetime)
    {
        byte[] tokenBytes = new byte[32];
        RandomNumberGenerator.Fill(tokenBytes);

        return new PasswordResetToken
        {
            Id = Guid.CreateVersion7(),
            AccountId = accountId,
            Token = Convert.ToBase64String(tokenBytes),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiryDateUtc = DateTime.UtcNow.Add(lifetime),
        };
    }

    public void MarkAsUsed()
    {
        UsedAtUtc ??= DateTime.UtcNow;
    }
}
