namespace Vast.Common.Application.IntegrationEvents;

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime OccurredAtUtc { get; }
}
