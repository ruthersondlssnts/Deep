using Vast.Accounts.Application.Features.Accounts;
using Vast.Common.Application.Authorization;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;

namespace Vast.Accounts.Application.Authorization;

internal sealed class PermissionService(IRequestBus requestBus) : IPermissionService
{
    public async Task<Result<PermissionsResponse>> GetUserPermissionsAsync(
        string identityId,
        CancellationToken cancellationToken = default
    ) =>
        await requestBus.Send<PermissionsResponse>(
            new GetAccountPermissions.Query(identityId),
            cancellationToken
        );
}
