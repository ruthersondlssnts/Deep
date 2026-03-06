using Deep.Common.Application.IntegrationEvents;

namespace Deep.Programs.IntegrationEvents;

public sealed class ProgramCancelledIntegrationEvent(
    Guid id,
    DateTime occurredAtUtc,
    Guid programId,
    string reason) : IntegrationEvent(id, occurredAtUtc)
{
    public Guid ProgramId { get; } = programId;
    public string Reason { get; } = reason;
}
