using Dapper;
using Deep.Common.Application.Dapper;
using Deep.Programs.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.Data.Repositories;

public class UserRepository(ProgramsDbContext db, IDbConnectionFactory dbConnectionFactory) : IUserRepository
{
    public async Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<bool> ExistWithRolesAsync(
        IReadOnlyCollection<(Guid UserId, string RoleName)> userRoles,
        CancellationToken cancellationToken = default)
    {
        if (userRoles.Count == 0)
        {
            return false;
        }

        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        // Use unnest to pass arrays and count matches in DB
        const string sql = """
            SELECT COUNT(DISTINCT u.id) = @Expected
            FROM programs.users u
            JOIN programs.user_roles ur ON ur.user_id = u.id
            WHERE (u.id, ur.role_name) IN (
                SELECT * FROM unnest(@UserIds::uuid[], @RoleNames::text[])
            )
            """;

        var userIds = userRoles.Select(r => r.UserId).ToArray();
        var roleNames = userRoles.Select(r => r.RoleName).ToArray();
        var expected = userRoles.Select(r => r.UserId).Distinct().Count();

        return await connection.QuerySingleAsync<bool>(sql, new
        {
            UserIds = userIds,
            RoleNames = roleNames,
            Expected = expected
        });
    }

    public void Insert(User user)
    {
        foreach (Role role in user.Roles)
        {
            db.Attach(role);
        }

        db.Users.Add(user);
    }
}
