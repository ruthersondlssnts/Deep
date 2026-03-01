using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using Dapper;
using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.Dapper;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.ProgramAssignments;
using Deep.Programs.Domain.Programs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.Features.Programs;

public static class UpdateProgram
{
    public sealed record ProgramUser([Required] Guid UserId, [Required] string RoleName);

    public sealed record Command(
        [Required] Guid ProgramId,
        [Required] string Name,
        [Required] string Description,
        [Required] DateTime StartsAtUtc,
        [Required] DateTime EndsAtUtc,
        [Required, MinLength(1, ErrorMessage = "At least one product is required.")]
            IReadOnlyCollection<string> ProductNames,
        [Required, MinLength(1, ErrorMessage = "At least one user is required.")]
            IReadOnlyCollection<ProgramUser> Users
    );

    public sealed record Response(Guid Id);

    public sealed class Handler(ProgramsDbContext context, IDbConnectionFactory dbConnectionFactory)
        : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command c, CancellationToken ct = default)
        {
            Program? program = await context
                .Programs.Include(p => p.Products)
                .SingleOrDefaultAsync(p => p.Id == c.ProgramId, ct);

            if (program is null)
            {
                return ProgramErrors.NotFound(c.ProgramId);
            }

            var assignments = c.Users.Select(u => (u.UserId, u.RoleName)).Distinct().ToList();

            if (!await ExistWithRolesAsync(assignments))
            {
                return ProgramErrors.ProgramUserNotFound;
            }

            Result updateResult = program.UpdateDetails(
                c.Name,
                c.Description,
                c.StartsAtUtc,
                c.EndsAtUtc,
                c.ProductNames,
                assignments
            );

            if (updateResult.IsFailure)
            {
                return updateResult.Error;
            }

            List<ProgramAssignment> existingAssignments = await context
                .ProgramAssignments.Where(a => a.ProgramId == c.ProgramId)
                .ToListAsync(ct);

            Result<IReadOnlyList<ProgramAssignment>> newAssignmentsResult =
                ProgramAssignment.UpdateAssignments(c.ProgramId, assignments, existingAssignments);

            if (newAssignmentsResult.IsFailure)
            {
                return newAssignmentsResult.Error;
            }

            if (newAssignmentsResult.Value.Count > 0)
            {
                context.ProgramAssignments.AddRange(newAssignmentsResult.Value);
            }

            await context.SaveChangesAsync(ct);

            return new Response(program.Id);
        }

        private async Task<bool> ExistWithRolesAsync(
            IReadOnlyCollection<(Guid UserId, string RoleName)> userRoles
        )
        {
            if (userRoles.Count == 0)
            {
                return false;
            }

            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

            const string sql = """
                SELECT COUNT(DISTINCT u.id) = @Expected
                FROM programs.users u
                JOIN programs.user_roles ur ON ur.user_id = u.id
                WHERE (u.id, ur.role_name) IN (
                    SELECT * FROM unnest(@UserIds::uuid[], @RoleNames::text[])
                )
                """;

            Guid[] userIds = userRoles.Select(r => r.UserId).ToArray();
            string[] roleNames = userRoles.Select(r => r.RoleName).ToArray();
            int expected = userRoles.Select(r => r.UserId).Distinct().Count();

            return await connection.QuerySingleAsync<bool>(
                sql,
                new
                {
                    UserIds = userIds,
                    RoleNames = roleNames,
                    Expected = expected,
                }
            );
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPut(
                    "/programs/{programId:guid}",
                    async (
                        Guid programId,
                        Command command,
                        IRequestHandler<Command, Response> handler,
                        CancellationToken ct
                    ) =>
                    {
                        Result<Response> result = await handler.Handle(
                            command with
                            {
                                ProgramId = programId,
                            },
                            ct
                        );

                        return result.Match(() => Results.Ok(result.Value), ApiResults.Problem);
                    }
                )
                .RequireAuthorization()
                .WithTags("Programs");
    }
}
