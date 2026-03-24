using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vast.Common.Application.Inbox;

public abstract partial class InboxBackgroundService<TProcessor>(
    IServiceScopeFactory scopeFactory,
    IInboxNotifier notifier,
    IOptions<InboxOptions> options,
    ILogger logger,
    string moduleName
) : BackgroundService
    where TProcessor : class, IInboxProcessor
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IInboxNotifier _notifier = notifier;
    private readonly InboxOptions _options = options.Value;
    private readonly ILogger _logger = logger;
    private readonly string _moduleName = moduleName;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogWorkerStarted(_logger, _moduleName, _options.IntervalInSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DrainAsync(stoppingToken);

                var delayTask = Task.Delay(
                    TimeSpan.FromSeconds(_options.IntervalInSeconds),
                    stoppingToken
                );

                Task signalTask = _notifier.WaitAsync(stoppingToken);

                Task completedTask = await Task.WhenAny(delayTask, signalTask);

                if (completedTask == signalTask)
                {
                    LogWokeUpBySignal(_logger, _moduleName);
                }
                else
                {
                    LogWokeUpByPolling(_logger, _moduleName);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                LogUnexpectedError(_logger, ex, _moduleName);

                await Task.Delay(TimeSpan.FromSeconds(_options.ErrorDelayInSeconds), stoppingToken);
            }
        }

        LogWorkerStopped(_logger, _moduleName);
    }

    private async Task DrainAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();

            TProcessor processor = scope.ServiceProvider.GetRequiredService<TProcessor>();

            int processed = await processor.ProcessAsync(token);

            if (processed == 0)
            {
                return;
            }
        }
    }

    [LoggerMessage(
        EventId = 7000,
        Level = LogLevel.Information,
        Message = "Inbox background worker started for module {ModuleName} with polling interval {IntervalInSeconds}s"
    )]
    private static partial void LogWorkerStarted(
        ILogger logger,
        string moduleName,
        int intervalInSeconds
    );

    [LoggerMessage(
        EventId = 7001,
        Level = LogLevel.Debug,
        Message = "Inbox worker woke up by signal for module {ModuleName}"
    )]
    private static partial void LogWokeUpBySignal(ILogger logger, string moduleName);

    [LoggerMessage(
        EventId = 7002,
        Level = LogLevel.Debug,
        Message = "Inbox worker woke up by polling for module {ModuleName}"
    )]
    private static partial void LogWokeUpByPolling(ILogger logger, string moduleName);

    [LoggerMessage(
        EventId = 7003,
        Level = LogLevel.Error,
        Message = "Unexpected inbox worker error for module {ModuleName}"
    )]
    private static partial void LogUnexpectedError(
        ILogger logger,
        Exception exception,
        string moduleName
    );

    [LoggerMessage(
        EventId = 7004,
        Level = LogLevel.Information,
        Message = "Inbox background worker stopped for module {ModuleName}"
    )]
    private static partial void LogWorkerStopped(ILogger logger, string moduleName);
}
