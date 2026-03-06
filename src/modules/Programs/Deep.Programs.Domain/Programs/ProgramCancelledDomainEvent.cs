using Deep.Common.Domain;

namespace Deep.Programs.Domain.Programs;

public sealed class ProgramCancelledDomainEvent(Guid programId, string reason) : DomainEvent
{
    public Guid ProgramId { get; } = programId;
    public string Reason { get; } = reason;
}
