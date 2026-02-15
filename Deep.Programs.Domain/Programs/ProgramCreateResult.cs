using Deep.Programs.Domain.ProgramAssignments;

namespace Deep.Programs.Domain.Programs;

public sealed record ProgramCreateResult(
    Program Program,
    IReadOnlyCollection<ProgramAssignment> Assignments
);
