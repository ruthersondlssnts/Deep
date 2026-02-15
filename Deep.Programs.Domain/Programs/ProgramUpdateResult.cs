using Deep.Common.Domain;
using Deep.Programs.Domain.ProgramAssignments;

namespace Deep.Programs.Domain.Programs;

public sealed record ProgramUpdateResult(
    Result Result,
    IReadOnlyList<ProgramAssignment> NewAssignments
);
