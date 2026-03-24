using Vast.Common.Domain;

namespace Vast.Programs.Domain.Programs;

public sealed class ProgramCreatedDomainEvent(Guid programId) : DomainEvent
{
    public Guid ProgramId { get; } = programId;
}
