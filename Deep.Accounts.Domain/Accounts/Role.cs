// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Common.Domain;

namespace Deep.Accounts.Domain.Accounts;

public sealed class Role
{
    public string Name { get; }

    public static readonly Role ItAdmin = new(RoleNames.ItAdmin);
    public static readonly Role ProgramOwner = new(RoleNames.ProgramOwner);
    public static readonly Role Manager = new(RoleNames.Manager);
    public static readonly Role BrandAmbassador = new(RoleNames.BrandAmbassador);
    public static readonly Role Coordinator = new(RoleNames.Coordinator);

    private static readonly IReadOnlyCollection<Role> All =
    [
        ItAdmin,
        ProgramOwner,
        Manager,
        BrandAmbassador,
        Coordinator
    ];

    private Role(string name)
    {
        Name = name;
    }

    public static bool TryFromName(string name, out Role role)
    {
        role = All.FirstOrDefault(r =>
            r.Name.Equals(name, StringComparison.OrdinalIgnoreCase))!;
        return role is not null;
    }

    public static bool TryFromName(string name) =>
        All.Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}
