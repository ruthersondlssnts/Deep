namespace Deep.Common.Application.EventBus;

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime OccurredAtUtc { get; }
}
