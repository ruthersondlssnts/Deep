using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deep.Common.Application.Inbox;

public abstract class InboxBackgroundService<TProcessor>(
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Inbox background worker started for module {ModuleName} with polling interval {IntervalInSeconds}s",
            moduleName,
            _options.IntervalInSeconds
        );

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

                await Task.WhenAny(delayTask, signalTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Inbox worker failure");

                await Task.Delay(TimeSpan.FromSeconds(_options.ErrorDelayInSeconds), stoppingToken);
            }
        }
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
}
