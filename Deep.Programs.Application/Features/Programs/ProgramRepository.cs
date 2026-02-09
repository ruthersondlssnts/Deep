using System.Data.Common;
using System.Text;
using Dapper;
using Deep.Common.Application.Dapper;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.Features.Programs;

public class ProgramRepository(IDbConnectionFactory dbConnectionFactory)
{
    public async Task<int> CountMatchingUserRolePairs(List<(Guid UserId, string RoleName)> users)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        var sql = new StringBuilder(
            @"SELECT COUNT(DISTINCT u.id)
                  FROM programs.users u
                  JOIN programs.user_roles ur ON ur.user_id = u.id
                  JOIN programs.roles r ON r.name = ur.role_name
                  WHERE (u.id, r.name) IN ("
        );

        var parameters = new Dapper.DynamicParameters();
        for (int i = 0; i < users.Count; i++)
        {
            sql.Append(i == 0 ? $"(@UserId{i}, @RoleName{i})" : $", (@UserId{i}, @RoleName{i})");
            parameters.Add($"UserId{i}", users[i].UserId);
            parameters.Add($"RoleName{i}", users[i].RoleName);
        }
        sql.Append(")");

        int validUserCount = await connection.ExecuteScalarAsync<int>(sql.ToString(), parameters);
        return validUserCount;
    }
}
