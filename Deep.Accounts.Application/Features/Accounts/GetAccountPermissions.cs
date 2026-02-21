using Dapper;
using Deep.Accounts.Domain.Accounts;
using Deep.Common.Application.Authorization;
using Deep.Common.Application.Dapper;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;

namespace Deep.Accounts.Application.Features.Accounts;

public static class GetAccountPermissions
{
    public sealed record Query(string IdentityId);

    public sealed class Handler(IDbConnectionFactory dbConnectionFactory) : IRequestHandler<Query, PermissionsResponse>
    {
        public async Task<Result<PermissionsResponse>> Handle(Query query, CancellationToken ct = default)
        {
            if (!Guid.TryParse(query.IdentityId, out Guid accountId))
            {
                return AccountErrors.NotFound(Guid.Empty);
            }

            await using var connection = await dbConnectionFactory.OpenConnectionAsync();

            const string sql = """
                SELECT DISTINCT
                    a.id AS AccountId,
                    rp.permission_code AS Permission
                FROM accounts.accounts a
                JOIN accounts.account_roles ar ON a.id = ar.account_id
                JOIN accounts.role_permissions rp ON ar.role_name = rp.role_name
                WHERE a.id = @AccountId
                """;

            var permissionResults = (await connection.QueryAsync<AccountPermission>(sql, new { AccountId = accountId }))
                .ToList();

            if (permissionResults.Count == 0)
            {
                return AccountErrors.NotFound(accountId);
            }

            return new PermissionsResponse(
                permissionResults[0].AccountId,
                permissionResults.Select(p => p.Permission).ToHashSet()
            );
        }
    }

    private sealed record AccountPermission(Guid AccountId, string Permission);
}
