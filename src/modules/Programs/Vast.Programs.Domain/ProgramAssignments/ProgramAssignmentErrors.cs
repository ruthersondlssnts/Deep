using Vast.Common.Domain;

namespace Vast.Programs.Domain.ProgramAssignments;

public static class ProgramAssignmentErrors
{
    public static Error NotFound(Guid assignmentId) =>
        Error.NotFound(
            "ProgramAssignment.NotFound",
            $"The program assignment with the identifier {assignmentId} was not found"
        );

    public static Error AlreadyInactive(Guid assignmentId) =>
        Error.Problem(
            "ProgramAssignment.AlreadyInactive",
            $"The program assignment {assignmentId} is already inactive"
        );

    public static readonly Error ProgramAssignmentAlreadyExists = Error.Problem(
        "ProgramAssignment.AlreadyExists",
        "The program assignment already exists"
    );

    public static readonly Error InvalidProgramAssignment = Error.Problem(
        "ProgramAssignment.InvalidProgramAssignment",
        "The program assignment is invalid"
    );

    public static readonly Error InvalidRole = Error.Problem(
        "ProgramAssignment.InvalidRole",
        "The role is invalid"
    );
}
