using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using Dapper;
using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.Authentication;
using Deep.Common.Application.Dapper;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.ProgramAssignments;
using Deep.Programs.Domain.Programs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Deep.Programs.Application.Features.Programs;

public static class CreateProgram
{
    public sealed record ProgramUser([Required] Guid UserId, [Required] string RoleName);

    public sealed record ProductRequest(
        [Required] string Sku,
        [Required] string Name,
        [Required, Range(0, double.MaxValue)] decimal UnitPrice,
        [Required, Range(0, int.MaxValue)] int Stock
    );

    public sealed record Command(
        [Required] string Name,
        [Required] string Description,
        [Required] DateTime StartsAtUtc,
        [Required] DateTime EndsAtUtc,
        [Required, MinLength(1, ErrorMessage = "At least one product is required.")]
            IReadOnlyCollection<ProductRequest> Products,
        [Required, MinLength(1, ErrorMessage = "At least one user is required.")]
            IReadOnlyCollection<ProgramUser> Users
    );

    public sealed record Response(Guid Id);

    public sealed class Handler(
        ProgramsDbContext context,
        IDbConnectionFactory dbConnectionFactory,
        IHttpContextAccessor httpContextAccessor
    ) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command c, CancellationToken ct = default)
        {
            Guid currentUserId = httpContextAccessor.HttpContext!.User.GetUserId();

            var assignments = c.Users.Select(u => (u.UserId, u.RoleName)).Distinct().ToList();

            if (!await ExistWithRolesAsync(assignments))
            {
                return ProgramErrors.ProgramUserNotFound;
            }

            var products = c
                .Products.Select(p => new ProductInput(p.Sku, p.Name, p.UnitPrice, p.Stock))
                .ToList();

            Result<Program> programResult = Program.Create(
                c.Name,
                c.Description,
                c.StartsAtUtc,
                c.EndsAtUtc,
                products,
                currentUserId,
                assignments
            );

            if (programResult.IsFailure)
            {
                return programResult.Error;
            }

            Result<IEnumerable<ProgramAssignment>> programAssignments =
                ProgramAssignment.CreateRange(programResult.Value.Id, assignments);

            if (programAssignments.IsFailure)
            {
                return programAssignments.Error;
            }

            await context.Programs.AddAsync(programResult.Value);
            await context.ProgramAssignments.AddRangeAsync(programAssignments.Value);

            await context.SaveChangesAsync(ct);

            return new Response(programResult.Value.Id);
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
            app.MapPost(
                    "/programs",
                    async (
                        Command command,
                        IRequestHandler<Command, Response> handler,
                        CancellationToken ct
                    ) =>
                    {
                        Result<Response> result = await handler.Handle(command, ct);

                        return result.Match(
                            () => Results.Created($"/programs/{result.Value.Id}", result.Value),
                            ApiResults.Problem
                        );
                    }
                )
                .RequireAuthorization()
                .WithTags("Programs");
    }
}
