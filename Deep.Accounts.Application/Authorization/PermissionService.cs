using Deep.Accounts.Application.Features.Accounts;
using Deep.Common.Application.Authorization;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;

namespace Deep.Accounts.Application.Authorization;

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
