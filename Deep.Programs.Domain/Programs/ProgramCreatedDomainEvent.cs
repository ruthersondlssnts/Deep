using Deep.Common.Domain;

namespace Deep.Programs.Domain.Programs;

public sealed class ProgramCreatedDomainEvent(Guid programId) : DomainEvent
{
    public Guid ProgramId { get; } = programId;
}
