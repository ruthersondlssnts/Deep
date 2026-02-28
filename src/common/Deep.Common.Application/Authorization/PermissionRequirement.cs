using Microsoft.AspNetCore.Authorization;

namespace Deep.Common.Application.Authorization;

internal sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
