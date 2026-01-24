using Deep.Common.Domain;

namespace Deep.Accounts.Domain.Accounts;

public sealed class Account : Entity
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public Role Role { get; private set; }

    private Account() { }

    public static Account Create(
        string firstName,
        string lastName,
        string email,
        Role role)
    {
        var user = new Account
        {
            Id = Guid.CreateVersion7(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Role = role
        };

        user.RaiseDomainEvent(new AccountRegisteredDomainEvent(user.Id));

        return user;
    }
}
