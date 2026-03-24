namespace Vast.Common.Application.Inbox;

public class InboxNotifier : IInboxNotifier, IDisposable
{
    private readonly SemaphoreSlim _signal = new(0);
    private bool _disposed;

    public void Notify() => _signal.Release();

    public Task WaitAsync(CancellationToken cancellationToken) =>
        _signal.WaitAsync(cancellationToken);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _signal.Dispose();
        }

        _disposed = true;
    }
}
