using Deep.Common.Application.Inbox;
using Deep.Common.Application.Outbox;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Deep.Common.Application.BackgroundJobs;

public static class InboxOutboxJobExtensions
{
    /// <summary>
    /// Registers recurring Hangfire jobs for inbox and outbox processing for a module.
    /// 
    /// NOTE: Hangfire recurring jobs have a minimum interval of 1 minute. For sub-minute polling,
    /// we use a simple "loop within job" pattern: the job runs every minute and processes
    /// messages in a loop with delays. This is the simplest production-stable approach.
    /// 
    /// Alternative approaches considered:
    /// 1. Self-scheduling jobs (BackgroundJob.Schedule at end of job) - more complex, harder to monitor
    /// 2. Hosted services with timers - doesn't leverage Hangfire's reliability/monitoring
    /// 
    /// The loop-within-job pattern keeps all processing visible in Hangfire dashboard while
    /// allowing configurable sub-minute effective intervals.
    /// </summary>
    public static IApplicationBuilder UseInboxOutboxJobs<TOutboxJob, TInboxJob>(
        this IApplicationBuilder app,
        string moduleName)
        where TOutboxJob : ProcessOutboxJobBase
        where TInboxJob : ProcessInboxJobBase
    {
        IRecurringJobManager recurringJobManager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();
        OutboxOptions outboxOptions = app.ApplicationServices.GetRequiredService<IOptions<OutboxOptions>>().Value;
        InboxOptions inboxOptions = app.ApplicationServices.GetRequiredService<IOptions<InboxOptions>>().Value;

        // Outbox job - runs every minute, processes in loop if interval < 60s
        recurringJobManager.AddOrUpdate<TOutboxJob>(
            $"{moduleName}:outbox",
            job => job.ProcessAsync(CancellationToken.None),
            GetCronExpression(outboxOptions.IntervalSeconds));

        // Inbox job - runs every minute, processes in loop if interval < 60s
        recurringJobManager.AddOrUpdate<TInboxJob>(
            $"{moduleName}:inbox",
            job => job.ProcessAsync(CancellationToken.None),
            GetCronExpression(inboxOptions.IntervalSeconds));

        return app;
    }

    /// <summary>
    /// Converts interval seconds to a Hangfire cron expression.
    /// Hangfire minimum is 1 minute, so intervals less than 60 seconds
    /// result in running every minute.
    /// </summary>
    private static string GetCronExpression(int intervalSeconds)
    {
        // Minimum Hangfire recurring job interval is 1 minute
        if (intervalSeconds < 60)
        {
            return Cron.Minutely();
        }

        if (intervalSeconds < 3600)
        {
            int minutes = intervalSeconds / 60;
            return $"*/{minutes} * * * *";
        }

        // For longer intervals, run hourly
        return Cron.Hourly();
    }
}
