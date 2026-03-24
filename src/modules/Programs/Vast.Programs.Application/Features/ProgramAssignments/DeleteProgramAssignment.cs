using System.ComponentModel.DataAnnotations;
using Vast.Common.Application.Api.ApiResults;
using Vast.Common.Application.Api.Endpoints;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;
using Vast.Programs.Application.Data;
using Vast.Programs.Domain.ProgramAssignments;
using Vast.Programs.Domain.Programs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Vast.Programs.Application.Features.ProgramAssignments;

public static class DeleteProgramAssignment
{
    public sealed record Command([Required] Guid AssignmentId);

    public sealed record Response(Guid Id);

    public sealed class Handler(ProgramsDbContext context) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(
            Command c,
            CancellationToken cancellationToken = default
        )
        {
            ProgramAssignment? assignment = await context.ProgramAssignments.SingleOrDefaultAsync(
                pa => pa.Id == c.AssignmentId,
                cancellationToken
            );

            if (assignment is null)
            {
                return ProgramAssignmentErrors.NotFound(c.AssignmentId);
            }

            if (!assignment.IsActive)
            {
                return ProgramAssignmentErrors.AlreadyInactive(c.AssignmentId);
            }

            ProgramStatus? programStatus = await context
                .Programs.Where(p => p.Id == assignment.ProgramId)
                .Select(p => (ProgramStatus?)p.ProgramStatus)
                .FirstOrDefaultAsync(cancellationToken);

            if (programStatus is null)
            {
                return ProgramErrors.NotFound(assignment.ProgramId);
            }

            List<ProgramAssignment> currentAssignments = await context
                .ProgramAssignments.Where(a => a.ProgramId == assignment.ProgramId && a.IsActive)
                .ToListAsync(cancellationToken);

            var assignmentsAfterDeactivation = currentAssignments
                .Where(a => a.Id != c.AssignmentId)
                .Select(a => (a.UserId, a.RoleName))
                .ToList();

            Result validation = Program.ValidateAssignments(
                assignmentsAfterDeactivation,
                programStatus.Value
            );

            if (validation.IsFailure)
            {
                return validation.Error;
            }

            assignment.Deactivate();

            await context.SaveChangesAsync(cancellationToken);

            return new Response(assignment.Id);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapDelete(
                    "/program-assignments/{id:guid}",
                    async (
                        Guid id,
                        IRequestHandler<Command, Response> handler,
                        CancellationToken ct
                    ) =>
                    {
                        Result<Response> result = await handler.Handle(new Command(id), ct);

                        return result.Match(Results.Ok, ApiResults.Problem);
                    }
                )
                .RequireAuthorization()
                .WithTags("ProgramAssignments");
    }
}
