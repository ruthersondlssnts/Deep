namespace Deep.Common.Application.Inbox;

public interface IInboxNotifier
{
    void Notify();
    Task WaitAsync(CancellationToken cancellationToken);
}
