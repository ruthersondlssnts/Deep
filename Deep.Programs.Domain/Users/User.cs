// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Common.Domain;

namespace Deep.Programs.Domain.Users;

public sealed class User : Entity
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
    private readonly List<Role> _roles = [];

    private User() { }

    public static User Create(
        Guid id,
        string firstName,
        string lastName,
        string email,
        IEnumerable<Role> roles)
    {
        var account = new User
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email
        };

        foreach (var role in roles)
        {
            account.AddRole(role);
        }

        return account;
    }

    private void AddRole(Role role)
    {
        if (_roles.Any(r => r.Name == role.Name))
            return;

        _roles.Add(role);
    }

    public bool HasRole(Role role) =>
       _roles.Any(r => r == role);
}
