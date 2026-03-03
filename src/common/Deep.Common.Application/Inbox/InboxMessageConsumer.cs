namespace Deep.Common.Application.Inbox;

public sealed class InboxMessageConsumer
{
    public required Guid InboxMessageId { get; init; }
    public required string Name { get; init; }
}
