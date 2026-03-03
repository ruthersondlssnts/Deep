namespace Deep.Common.Application.Inbox;

public sealed class InboxOptions
{
    public int IntervalInSeconds { get; init; } = 10;
    public int BatchSize { get; init; } = 100;
}
