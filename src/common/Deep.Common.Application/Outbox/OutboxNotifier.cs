namespace Deep.Common.Application.Outbox;

public class OutboxNotifier : IOutboxNotifier, IDisposable
{
    private readonly SemaphoreSlim _signal = new(0);

    public void Notify() => _signal.Release();

    public Task WaitAsync(CancellationToken cancellationToken) =>
        _signal.WaitAsync(cancellationToken);

    public void Dispose() => _signal.Dispose();
}
