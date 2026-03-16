namespace Deep.Common.Application.Outbox;

public sealed class OutboxOptions
{
    public int IntervalInSeconds { get; init; } = 60;
    public int BatchSize { get; init; } = 100;
    public int ErrorDelayInSeconds { get; init; } = 10;
}
