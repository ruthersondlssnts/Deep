namespace Deep.Common.Application.BackgroundJobs;

public sealed class HangfireOptions
{
    public const string SectionName = "Hangfire";

    public string? ConnectionString { get; set; }

    public string Schema { get; set; } = "hangfire";

    public int WorkerCount { get; set; } = 5;

    public bool EnableDashboard { get; set; } = true;
}
