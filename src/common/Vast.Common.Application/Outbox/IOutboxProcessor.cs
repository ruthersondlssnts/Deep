namespace Vast.Common.Application.Outbox;

public interface IOutboxProcessor
{
    Task<int> ProcessAsync(CancellationToken cancellationToken = default);
}
