using Deep.Common.Domain;

namespace Deep.Accounts.Domain.Accounts;

public sealed class Account : Entity
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
    private readonly List<Role> _roles = [];

    private Account() { }

    public static Account Create(
        string firstName,
        string lastName,
        string email,
        IEnumerable<Role> roles
    )
    {
        var account = new Account
        {
            Id = Guid.CreateVersion7(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
        };

        foreach (var role in roles)
        {
            account.AddRole(role);
        }

        account.RaiseDomainEvent(new AccountRegisteredDomainEvent(account.Id));

        return account;
    }

    private void AddRole(Role role)
    {
        if (_roles.Any(r => r.Name == role.Name))
            return;

        _roles.Add(role);
    }
}
