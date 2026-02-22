using System.Data.Common;
using System.Text;
using Dapper;
using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.Dapper;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Deep.Programs.Application.Features.ProgramAssignments;

public static class GetProgramAssignments
{
    public sealed record Query(Guid? UserId, Guid? ProgramId);

    public sealed record Response(
        Guid AssignmentId,
        Guid ProgramId,
        Guid UserId,
        string ProgramName,
        string ProgramDescription,
        DateTime StartsAtUtc,
        DateTime EndsAtUtc,
        string Role,
        string Firstname,
        string Lastname,
        bool isActive
    );

    public sealed class Handler(IDbConnectionFactory dbConnectionFactory)
        : IRequestHandler<Query, IReadOnlyList<Response>>
    {
        public async Task<Result<IReadOnlyList<Response>>> Handle(
            Query request,
            CancellationToken cancellationToken
= default
        )
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

            var sql = new StringBuilder(
                $"""
                    SELECT
                        pa.id             AS {nameof(Response.AssignmentId)},
                        p.id              AS {nameof(Response.ProgramId)},
                        u.id              AS {nameof(Response.UserId)},
                        p.name            AS {nameof(Response.ProgramName)},
                        p.description     AS {nameof(Response.ProgramDescription)},
                        p.starts_at_utc   AS {nameof(Response.StartsAtUtc)},
                        p.ends_at_utc     AS {nameof(Response.EndsAtUtc)},
                        pa.role_name      AS {nameof(Response.Role)},
                        u.first_name      AS {nameof(Response.Firstname)},
                        u.last_name       AS {nameof(Response.Lastname)},    
                        pa.is_active      AS {nameof(Response.isActive)}
                    FROM programs.program_assignments pa
                    INNER JOIN programs.programs p
                        ON p.id = pa.program_id
                    INNER JOIN programs.users u
                        ON u.id = pa.user_id
                    WHERE 1 = 1
                """
            );

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
            List<Response> assignments = (
                await connection.QueryAsync<Response>(sql.ToString(), parameters)
            ).AsList();

            return assignments;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapGet(
                    "/program-assignments",
                    async (
                        Guid? userId,
                        Guid? programId,
                        IRequestHandler<Query, IReadOnlyList<Response>> handler,
                        CancellationToken ct
                    ) =>
                    {
                        Result<IReadOnlyList<Response>> result = await handler.Handle(
                            new Query(userId, programId),
                            ct
                        );

                        return result.Match(Results.Ok, ApiResults.Problem);
                    }
                )
                .WithTags("ProgramAssignments");
    }
}
