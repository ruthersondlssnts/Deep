namespace Vast.Transactions.Domain.Transaction;

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled,
    Refunded,
}
