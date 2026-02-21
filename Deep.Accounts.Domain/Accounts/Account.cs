using Deep.Common.Domain;

namespace Deep.Accounts.Domain.Accounts;

public sealed class Account : Entity
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string SecurityStamp { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
    private readonly List<Role> _roles = [];

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    private readonly List<RefreshToken> _refreshTokens = [];

    private Account() { }

    public static Result<Account> Create(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        IReadOnlyCollection<string> roleNames
    )
    {
        var account = new Account
        {
            Id = Guid.CreateVersion7(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = passwordHash,
            SecurityStamp = Guid.CreateVersion7().ToString(),
            IsActive = true,
        };

        Result<IReadOnlyCollection<Role>> roles = CreateRolesFromNames(roleNames);

        if (roles.IsFailure)
        {
            return roles.Error;
        }

        foreach (Role role in roles.Value)
        {
            account.AddRole(role);
        }

        account.RaiseDomainEvent(new AccountRegisteredDomainEvent(account.Id));

        return account;
    }

    public RefreshToken AddRefreshToken(TimeSpan lifetime)
    {
        RefreshToken refreshToken = RefreshToken.Create(Id, lifetime);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public void RevokeRefreshToken(RefreshToken token)
    {
        token.Revoke();
    }

    public void RevokeAllRefreshTokens()
    {
        foreach (RefreshToken token in _refreshTokens.Where(t => t.IsActive))
        {
            token.Revoke();
        }
    }

    public RefreshToken? GetActiveRefreshToken(string token)
    {
        return _refreshTokens.FirstOrDefault(t => t.Token == token && t.IsActive);
    }

    public void UpdateSecurityStamp()
    {
        SecurityStamp = Guid.CreateVersion7().ToString();
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdateSecurityStamp();
    }

    public void Activate() => IsActive = true;

    public void Deactivate()
    {
        IsActive = false;
        RevokeAllRefreshTokens();
    }

    private void AddRole(Role role)
    {
        if (_roles.Any(r => r.Name == role.Name))
        {
            return;
        }

        _roles.Add(role);
    }

    public static Result<IReadOnlyCollection<Role>> CreateRolesFromNames(
        IReadOnlyCollection<string> roleNames
    )
    {
        var roles = new List<Role>();
        foreach (string roleName in roleNames)
        {
            if (!Role.TryFromName(roleName, out Role? role))
            {
                return AccountErrors.InvalidRole;
            }
            roles.Add(role);
        }
        return roles;
    }
}
