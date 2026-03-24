namespace Vast.Common.Application.Inbox;

public interface IInboxNotifier
{
    void Notify();
    Task WaitAsync(CancellationToken cancellationToken);
}
