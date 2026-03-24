using Vast.Common.Domain;

namespace Vast.Programs.Domain.ProgramAssignments;

public sealed class ProgramAssignmentDeactivatedDomainEvent(Guid programId, Guid userId)
    : DomainEvent
{
    public Guid ProgramId { get; } = programId;
    public Guid UserId { get; } = userId;
}
