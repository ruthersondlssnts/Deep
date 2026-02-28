using Deep.Common.Domain;
using Deep.Common.Domain.Auditing;

namespace Deep.Programs.Domain.Users;

[Auditable]
public sealed class User : Entity
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
    private readonly List<Role> _roles = [];

    private User() { }

    public static Result<User> Create(
        Guid id,
        string firstName,
        string lastName,
        string email,
        IReadOnlyCollection<string> roleNames
    )
    {
        var account = new User
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
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

        return account;
    }

    private void AddRole(Role role)
    {
        if (_roles.Any(r => r.Name == role.Name))
        {
            return;
        }

        _roles.Add(role);
    }

    public bool HasRole(Role role) => _roles.Any(r => r == role);

    public static Result<IReadOnlyCollection<Role>> CreateRolesFromNames(
        IReadOnlyCollection<string> roleNames
    )
    {
        var roles = new List<Role>();
        foreach (string roleName in roleNames)
        {
            if (!Role.TryFromName(roleName, out Role? role))
            {
                return UserErrors.InvalidRole;
            }
            roles.Add(role);
        }
        return roles;
    }
}
