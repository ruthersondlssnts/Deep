namespace Deep.Common.Application.Outbox;

public sealed class OutboxMessageConsumer
{
    public required Guid OutboxMessageId { get; init; }
    public required string Name { get; init; }
}
