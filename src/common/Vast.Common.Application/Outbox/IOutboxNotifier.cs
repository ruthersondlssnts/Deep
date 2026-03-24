namespace Vast.Common.Application.Outbox;

public interface IOutboxNotifier
{
    void Notify();
    Task WaitAsync(CancellationToken cancellationToken);
}
