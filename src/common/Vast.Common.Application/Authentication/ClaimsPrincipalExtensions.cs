using System.Security.Claims;
using Vast.Common.Application.Exceptions;

namespace Vast.Common.Application.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        string? userId = principal?.FindFirstValue(CustomClaims.Sub);

        return Guid.TryParse(userId, out Guid parsedUserId)
            ? parsedUserId
            : throw new VastException("User identifier is unavailable");
    }

    public static string GetIdentityId(this ClaimsPrincipal? principal) =>
        principal?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new VastException("User identity is unavailable");

    public static HashSet<string> GetPermissions(this ClaimsPrincipal? principal)
    {
        IEnumerable<Claim> permissionClaims =
            principal?.FindAll(CustomClaims.Permission)
            ?? throw new VastException("Permissions are unavailable");
        return permissionClaims.Select(claim => claim.Value).ToHashSet();
    }

    public static HashSet<string> GetRoles(this ClaimsPrincipal? principal)
    {
        IEnumerable<Claim> roleClaims =
            principal?.FindAll(ClaimTypes.Role) ?? throw new VastException("Roles are unavailable");
        return roleClaims.Select(claim => claim.Value).ToHashSet();
    }
}
