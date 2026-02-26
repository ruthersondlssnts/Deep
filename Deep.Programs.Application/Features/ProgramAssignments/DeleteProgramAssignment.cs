using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.ProgramAssignments;
using Deep.Programs.Domain.Programs;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Deep.Programs.Application.Features.ProgramAssignments;

public static class DeleteProgramAssignment
{
    public sealed record Command(Guid AssignmentId);

    public sealed record Response(Guid Id);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator() => RuleFor(x => x.AssignmentId).NotEmpty();
    }

    public sealed class Handler(
        ProgramsDbContext context,
        IProgramAssignmentRepository assignmentRepository,
        IProgramRepository programRepository
    ) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command c, CancellationToken ct = default)
        {
            ProgramAssignment? assignment = await assignmentRepository.GetAsync(c.AssignmentId, ct);

            if (assignment is null)
            {
                return ProgramAssignmentErrors.NotFound(c.AssignmentId);
            }

            if (!assignment.IsActive)
            {
                return ProgramAssignmentErrors.AlreadyInactive(c.AssignmentId);
            }

            List<ProgramAssignment> currentAssignments =
                await assignmentRepository.GetActiveAssignmentsByProgramId(
                    assignment.ProgramId,
                    ct
                );

            var assignmentsAfterDeactivation = currentAssignments
                .Where(a => a.Id != c.AssignmentId)
                .Select(a => (a.UserId, a.RoleName))
                .ToList();

            Program? program = await programRepository.GetAsync(assignment.ProgramId, ct);
            if (program is null)
            {
                return ProgramErrors.NotFound(assignment.ProgramId);
            }

            Result validation = Program.ValidateAssignments(
                assignmentsAfterDeactivation,
                program.ProgramStatus
            );

            if (validation.IsFailure)
            {
                return validation.Error;
            }

            assignment.Deactivate();

            await context.SaveChangesAsync(ct);

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
