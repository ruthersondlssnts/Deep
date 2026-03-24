using Microsoft.AspNetCore.Authorization;

namespace Vast.Common.Application.Authorization;

internal sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
