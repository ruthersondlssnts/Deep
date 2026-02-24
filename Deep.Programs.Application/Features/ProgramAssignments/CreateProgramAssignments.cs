using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.ProgramAssignments;
using FluentValidation;

namespace Deep.Programs.Application.Features.ProgramAssignments;

public static class CreateProgramAssignments
{
    public sealed record Command(
        Guid ProgramId,
        IReadOnlyCollection<(Guid UserId, string RoleName)> Assignments
    );

    public sealed record Response(int CreatedCount);

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
            Result<IEnumerable<ProgramAssignment>> result = ProgramAssignment.CreateRange(
                c.ProgramId,
                c.Assignments
            );

            if (result.IsFailure)
            {
                return result.Error;
            }

            var assignments = result.Value.ToList();
            programAssignmentRepository.InsertRange(assignments);

            await context.SaveChangesAsync(ct);

            return new Response(assignments.Count);
        }
    }
}
