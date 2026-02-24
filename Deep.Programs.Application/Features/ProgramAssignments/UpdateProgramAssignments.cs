using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.ProgramAssignments;
using FluentValidation;

namespace Deep.Programs.Application.Features.ProgramAssignments;

public static class UpdateProgramAssignments
{
    public sealed record Command(
        Guid ProgramId,
        IReadOnlyCollection<(Guid UserId, string RoleName)> Assignments
    );

    public sealed record Response(int CreatedCount, int UpdatedCount);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ProgramId).NotEmpty();
            RuleFor(x => x.Assignments).NotEmpty();
        }
    }

    public sealed class Handler(
        ProgramsDbContext context,
        IProgramAssignmentRepository programAssignmentRepository
    ) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command c, CancellationToken ct = default)
        {
            List<ProgramAssignment> existingAssignments = 
                await programAssignmentRepository.GetAssignmentsByProgramId(c.ProgramId, ct);

            Result<IReadOnlyList<ProgramAssignment>> result = ProgramAssignment.UpdateAssignments(
                c.ProgramId,
                c.Assignments.ToList(),
                existingAssignments
            );

            if (result.IsFailure)
            {
                return result.Error;
            }

            var newAssignments = result.Value;
            if (newAssignments.Count > 0)
            {
                programAssignmentRepository.InsertRange(newAssignments);
            }

            await context.SaveChangesAsync(ct);

            // Count updated (reactivated) assignments
            int updatedCount = existingAssignments.Count(a => 
                c.Assignments.Any(req => req.UserId == a.UserId && req.RoleName == a.RoleName));

            return new Response(newAssignments.Count, updatedCount);
        }
    }
}
