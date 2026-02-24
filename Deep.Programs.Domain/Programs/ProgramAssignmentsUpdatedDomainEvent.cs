using Deep.Common.Domain;

namespace Deep.Programs.Domain.Programs;

public sealed class ProgramAssignmentsUpdatedDomainEvent(
    Guid programId,
    IReadOnlyCollection<(Guid UserId, string RoleName)> assignments
) : DomainEvent
{
    public Guid ProgramId { get; } = programId;
    public IReadOnlyCollection<(Guid UserId, string RoleName)> Assignments { get; } = assignments;
}
