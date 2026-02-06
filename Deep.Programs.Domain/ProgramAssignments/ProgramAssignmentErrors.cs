using Deep.Common.Domain;

namespace Deep.Programs.Domain.ProgramAssignments;

public static class ProgramAssignmentErrors
{
    public static Error NotFound(Guid programId) =>
        Error.NotFound("ProgramAssignment.NotFound", $"The program assignment with the identifier {programId} was not found");

    public static readonly Error ProgramAssignmentAlreadyExists = Error.Problem(
        "ProgramAssignment.AlreadyExists",
        "The program assignment already exists");

    public static Error InvalidProgramAssignment = Error.Problem(
       "ProgramAssignment.InvalidProgramAssignment",
       "The program assignment is invalid");

    public static readonly Error InvalidRole = Error.Problem(
          "ProgramAssignment.InvalidRole",
          "The role is invalid");
}
