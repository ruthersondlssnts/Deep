using Deep.Common.Application.Inbox;
using Deep.Common.Application.Outbox;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Deep.Common.Application.BackgroundJobs;

public static class InboxOutboxJobExtensions
{
    public static IApplicationBuilder UseInboxOutboxJobs<TOutboxJob, TInboxJob>(
        this IApplicationBuilder app,
        string moduleName)
        where TOutboxJob : ProcessOutboxJobBase
        where TInboxJob : ProcessInboxJobBase
    {
        IRecurringJobManager recurringJobManager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();
        OutboxOptions outboxOptions = app.ApplicationServices.GetRequiredService<IOptions<OutboxOptions>>().Value;
        InboxOptions inboxOptions = app.ApplicationServices.GetRequiredService<IOptions<InboxOptions>>().Value;

        recurringJobManager.AddOrUpdate<TOutboxJob>(
            $"{moduleName}:outbox",
            job => job.ProcessAsync(CancellationToken.None),
            GetCronExpression(outboxOptions.IntervalInSeconds));

        recurringJobManager.AddOrUpdate<TInboxJob>(
            $"{moduleName}:inbox",
            job => job.ProcessAsync(CancellationToken.None),
            GetCronExpression(inboxOptions.IntervalInSeconds));

        return app;
    }

    private static string GetCronExpression(int intervalSeconds)
    {
        if (intervalSeconds < 60)
        {
            return Cron.Minutely();
        }

        if (intervalSeconds < 3600)
        {
            return $"*/{intervalSeconds / 60} * * * *";
        }

        return Cron.Hourly();
    }
}
