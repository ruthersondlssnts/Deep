using Vast.Common.Domain;

namespace Vast.Common.Application.Authorization;

public interface IPermissionService
{
    Task<Result<PermissionsResponse>> GetUserPermissionsAsync(
        string identityId,
        CancellationToken cancellationToken = default
    );
}

public sealed record PermissionsResponse(Guid UserId, HashSet<string> Permissions);
