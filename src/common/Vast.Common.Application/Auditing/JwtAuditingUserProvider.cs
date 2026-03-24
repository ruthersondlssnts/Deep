using System.Security.Claims;
using Vast.Common.Application.Authentication;
using Microsoft.AspNetCore.Http;

namespace Vast.Common.Application.Auditing;

public sealed class JwtAuditingUserProvider(IHttpContextAccessor httpContextAccessor)
    : IAuditingUserProvider
{
    public Guid? GetCurrentUserId()
    {
        ClaimsPrincipal? principal = httpContextAccessor.HttpContext?.User;

        if (principal is null)
        {
            return null;
        }

        string? userId = principal.FindFirstValue(CustomClaims.Sub);

        if (Guid.TryParse(userId, out Guid parsedUserId))
        {
            return parsedUserId;
        }

        return null;
    }
}
