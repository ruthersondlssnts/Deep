namespace Deep.Common.Application.BackgroundJobs;

public sealed class HangfireOptions
{
    public const string SectionName = "Hangfire";

    /// <summary>
    /// PostgreSQL connection string for Hangfire storage.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Schema name for Hangfire tables.
    /// </summary>
    public string Schema { get; set; } = "hangfire";

    /// <summary>
    /// Number of worker threads.
    /// </summary>
    public int WorkerCount { get; set; } = 5;

    /// <summary>
    /// Enable Hangfire dashboard.
    /// </summary>
    public bool EnableDashboard { get; set; } = true;
}
