using Vast.Common.Domain;

namespace Vast.Programs.Domain.Programs;

public sealed class ProgramUpdatedDomainEvent(Guid programId) : DomainEvent
{
    public Guid ProgramId { get; } = programId;
}
