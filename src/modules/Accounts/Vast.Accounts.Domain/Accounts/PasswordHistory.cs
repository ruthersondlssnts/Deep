namespace Vast.Accounts.Domain.Accounts;

public sealed class PasswordHistory
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime ChangedAtUtc { get; private set; }

    private PasswordHistory() { }

    public static PasswordHistory Create(Guid accountId, string passwordHash) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            AccountId = accountId,
            PasswordHash = passwordHash,
            ChangedAtUtc = DateTime.UtcNow,
        };
}
