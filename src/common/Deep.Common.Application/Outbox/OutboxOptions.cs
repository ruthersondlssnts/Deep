namespace Deep.Common.Application.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    /// <summary>
    /// Interval in seconds between outbox processing batches.
    /// </summary>
    public int IntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Number of messages to process per batch.
    /// </summary>
    public int BatchSize { get; set; } = 100;
}
