using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vast.Common.Application.Outbox;

public abstract partial class OutboxBackgroundService<TProcessor>(
    IServiceScopeFactory serviceScopeFactory,
    IOutboxNotifier notifier,
    IOptions<OutboxOptions> options,
    ILogger logger,
    string moduleName
) : BackgroundService
    where TProcessor : class, IOutboxProcessor
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IOutboxNotifier _notifier = notifier;
    private readonly OutboxOptions _options = options.Value;
    private readonly ILogger _logger = logger;
    private readonly string _moduleName = moduleName;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogWorkerStarted(_logger, _moduleName, _options.IntervalInSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DrainUntilEmptyAsync(stoppingToken);

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

    private async Task DrainUntilEmptyAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();

            TProcessor processor = scope.ServiceProvider.GetRequiredService<TProcessor>();

            int processedCount = await processor.ProcessAsync(cancellationToken);

            if (processedCount == 0)
            {
                return;
            }

            if (processedCount < _options.BatchSize)
            {
                return;
            }
        }
    }

    [LoggerMessage(
        EventId = 4000,
        Level = LogLevel.Information,
        Message = "Outbox background worker started for module {ModuleName} with polling interval {IntervalInSeconds}s"
    )]
    private static partial void LogWorkerStarted(
        ILogger logger,
        string moduleName,
        int intervalInSeconds
    );

    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Debug,
        Message = "Outbox worker woke up by signal for module {ModuleName}"
    )]
    private static partial void LogWokeUpBySignal(ILogger logger, string moduleName);

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Debug,
        Message = "Outbox worker woke up by polling for module {ModuleName}"
    )]
    private static partial void LogWokeUpByPolling(ILogger logger, string moduleName);

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Error,
        Message = "Unexpected outbox worker error for module {ModuleName}"
    )]
    private static partial void LogUnexpectedError(
        ILogger logger,
        Exception exception,
        string moduleName
    );

    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Information,
        Message = "Outbox background worker stopped for module {ModuleName}"
    )]
    private static partial void LogWorkerStopped(ILogger logger, string moduleName);
}
