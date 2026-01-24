using Dapper;
using Deep.Common.Api.ApiResults;
using Deep.Common.Api.Endpoints;
using Deep.Common.Dapper;
using Deep.Common.Domain;
using Deep.Common.SimpleMediatR;
using Deep.Programs.Domain.ProgramAssignments;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text;

namespace Deep.Programs.Features.ProgramAssignments
{
    public static class GetProgramAssignments
    {
        public sealed record Query(
            Guid? UserId,
            Guid? ProgramId);

        public sealed record Response(
            Guid AssignmentId,
            Guid ProgramId,
            string ProgramName,
            string ProgramDescription,
            DateTime StartsAtUtc,
            DateTime EndsAtUtc,
            Role Role,
            string Firstname,
            string Lastname,
            bool isActive);

        public sealed class Handler(
            IDbConnectionFactory dbConnectionFactory)
            : IRequestHandler<Query, IReadOnlyList<Response>>
        {
            public async Task<Result<IReadOnlyList<Response>>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                await using var connection =
                    await dbConnectionFactory.OpenConnectionAsync();

                var sql = new StringBuilder(
                   $"""
                        SELECT
                            pa.id             AS {nameof(Response.AssignmentId)},
                            p.id              AS {nameof(Response.ProgramId)},
                            p.name            AS {nameof(Response.ProgramName)},
                            p.description     AS {nameof(Response.ProgramDescription)},
                            p.starts_at_utc   AS {nameof(Response.StartsAtUtc)},
                            p.ends_at_utc     AS {nameof(Response.EndsAtUtc)},
                            u.role            AS {nameof(Response.Role)},
                            u.first_name      AS {nameof(Response.Firstname)},
                            u.last_name       AS {nameof(Response.Lastname)},    
                            u.is_active       AS {nameof(Response.isActive)}
                        FROM programs.program_assignments pa
                        INNER JOIN programs.programs p
                            ON p.id = pa.program_id
                        INNER JOIN programs.users u
                            ON u.id = pa.user_id
                        WHERE 1 = 1
                    """);

                var parameters = new DynamicParameters();

                if (request.UserId.HasValue)
                {
                    sql.Append(" AND pa.user_id = @UserId");
                    parameters.Add("UserId", request.UserId);
                }

                if (request.ProgramId.HasValue)
                {
                    sql.Append(" AND pa.program_id = @ProgramId");
                    parameters.Add("ProgramId", request.ProgramId);
                }

                sql.Append(" ORDER BY p.starts_at_utc");
                var assignments = (await connection
                        .QueryAsync<Response>(
                            sql.ToString(),
                            parameters))
                    .AsList();

                return assignments;
            }
        }

        public sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app) =>
                app.MapGet("/program-assignments", async (
                    Guid? userId,
                    Guid? programId,
                    IRequestHandler<Query, IReadOnlyList<Response>> handler,
                    CancellationToken ct) =>
                {
                    var result = await handler.Handle(
                        new Query(userId, programId), ct);

                    return result.Match(
                        Results.Ok,
                        ApiResults.Problem);
                })
                .WithTags("ProgramAssignments");
        }
    }

}
