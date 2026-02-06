using Deep.Common.Domain;

namespace Deep.Programs.Domain.ProgramAssignments;

public sealed class ProgramAssignmentDeactivatedDomainEvent(
    Guid programId, Guid userId) : DomainEvent
{
    public Guid ProgramId { get; } = programId;
    public Guid UserId { get; } = userId;
}
