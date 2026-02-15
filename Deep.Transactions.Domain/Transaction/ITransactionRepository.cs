namespace Deep.Transactions.Domain.Transaction;

public interface ITransactionRepository
{
    Task<Transaction?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    void Insert(Transaction @event);
}
