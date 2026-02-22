namespace Deep.Accounts.Domain.Accounts;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiryDateUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public Guid AccountId { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiryDateUtc;
    public bool IsRevoked => RevokedAtUtc is not null;
    public bool IsActive => !IsExpired && !IsRevoked;

    private RefreshToken() { }

    public static RefreshToken Create(Guid accountId, TimeSpan lifetime) =>
        new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            Token = Convert.ToBase64String(Guid.CreateVersion7().ToByteArray()),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiryDateUtc = DateTime.UtcNow.Add(lifetime),
            AccountId = accountId,
        };

    public void Revoke()
    {
        RevokedAtUtc ??= DateTime.UtcNow;
    }
}
