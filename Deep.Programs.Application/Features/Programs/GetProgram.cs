using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.Programs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.Features.Programs;
public static class GetProgram
{
    public sealed record Query(Guid Id);

    public sealed record Response(
        Guid Id,
        string Name,
        string Description,
        DateTime StartsAtUtc,
        DateTime EndsAtUtc,
        ProgramStatus ProgramStatus,
        Guid OwnerId,
        string OwnerName,
        IReadOnlyList<string> Products,
        IReadOnlyList<ProgramAssignmentResponse> Assignments);

    public sealed record ProgramAssignmentResponse(
        Guid UserId,
        string FullName,
        string Email,
        Role Role,
        string RoleName);

    public sealed class Handler(ProgramsDbContext context)
        : IRequestHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(
            Query query,
            CancellationToken ct = default)
        {
            var program = await context.Programs
                .AsNoTracking()
                .Where(p => p.Id == query.Id)
                .Select(p => new Response(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.StartsAtUtc,
                    p.EndsAtUtc,
                    p.ProgramStatus,
                    p.OwnerId,

                    // Owner name
                    context.Users
                        .Where(u => u.Id == p.OwnerId)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault()!,
                   p.Products
                        .Select(pp => pp.ProductName)
                        .ToList(),
                    // Assignments
                    context.ProgramAssignments
                        .Where(pa => pa.ProgramId == p.Id && pa.IsActive)
                        .Join(
                            context.Users,
                            pa => pa.UserId,
                            u => u.Id,
                            (pa, u) => new ProgramAssignmentResponse(
                                u.Id,
                                u.FirstName + " " + u.LastName,
                                u.Email,
                                pa.Role,
                                pa.Role.ToString()
                            ))
                        .ToList()
                ))
                .FirstOrDefaultAsync(ct);

            return program is null
                ? ProgramErrors.NotFound(query.Id)
                : program;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/programs/{id:guid}", async (
                Guid id,
                IRequestHandler<Query, Response> handler,
                CancellationToken ct) =>
            {
                var result = await handler.Handle(
                    new Query(id), ct);

                return result.Match(
                    Results.Ok,
                    ApiResults.Problem);
            })
            .WithTags("Programs");
        }
    }
}
