using System.Text;
using Deep.Programs.Domain.Programs;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.Data.Repositories;

public class ProgramRepository(ProgramsDbContext db) : IProgramRepository
{
    public async Task<Program?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db
            .Programs.Include(p => p.Products)
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

    public void Insert(Program program) => db.Programs.Add(program);

    public async Task<bool> AreAllUsersValid(
        List<(Guid UserId, string RoleName)> assignmentPairs,
        CancellationToken cancellationToken = default
    )
    {
        if (assignmentPairs == null || assignmentPairs.Count == 0)
        {
            return false;
        }

        var sql = new StringBuilder(
            @"
        SELECT (
            COUNT(DISTINCT u.id) = @expected
        ) AS ""Value""
        FROM programs.users u
        JOIN programs.user_roles ur ON ur.user_id = u.id
        JOIN programs.roles r ON r.name = ur.role_name
        WHERE (u.id, r.name) IN (
    "
        );

        var parameters = new List<object>();

        for (int i = 0; i < assignmentPairs.Count; i++)
        {
            if (i > 0)
            {
                sql.Append(", ");
            }

            sql.Append($"(@user{i}, @role{i})");

            parameters.Add(new Npgsql.NpgsqlParameter($"user{i}", assignmentPairs[i].UserId));
            parameters.Add(new Npgsql.NpgsqlParameter($"role{i}", assignmentPairs[i].RoleName));
        }

        sql.Append(")");

        int expected = assignmentPairs.Select(x => x.UserId).Distinct().Count();

        parameters.Add(new Npgsql.NpgsqlParameter("expected", expected));

        return await db
            .Database.SqlQueryRaw<bool>(sql.ToString(), parameters.ToArray())
            .FirstAsync(cancellationToken);
    }
}
