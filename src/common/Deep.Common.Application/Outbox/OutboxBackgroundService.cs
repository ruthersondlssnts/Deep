using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deep.Common.Application.Outbox;

public abstract class OutboxBackgroundService<TProcessor>(
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
        _logger.LogInformation(
            "Outbox background worker started for module {ModuleName} with polling interval {IntervalInSeconds}s",
            _moduleName,
            _options.IntervalInSeconds
        );

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

                _logger.LogDebug(
                    completedTask == signalTask
                        ? "Outbox worker woke up by signal for module {ModuleName}"
                        : "Outbox worker woke up by polling for module {ModuleName}",
                    _moduleName
                );
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected outbox worker error for module {ModuleName}",
                    _moduleName
                );

                await Task.Delay(TimeSpan.FromSeconds(_options.ErrorDelayInSeconds), stoppingToken);
            }
        }

        _logger.LogInformation(
            "Outbox background worker stopped for module {ModuleName}",
            _moduleName
        );
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
}
