namespace Deep.Common.Application.Inbox;

public sealed class InboxOptions
{
    public const string SectionName = "Inbox";

    /// <summary>
    /// Interval in seconds between inbox processing batches.
    /// </summary>
    public int IntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Number of messages to process per batch.
    /// </summary>
    public int BatchSize { get; set; } = 100;
}
