using Deep.Common.Domain;

namespace Deep.Transactions.Domain.Transaction;

public sealed class Transaction : Entity
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ProgramId { get; private set; }

    private Transaction() { }

    public static Result<Transaction> Create(
        Guid programId,
        Guid customerId)
    {
        var transaction = new Transaction
        {
            Id = Guid.CreateVersion7(),
            CustomerId = customerId,
            ProgramId = programId
        };

        transaction.RaiseDomainEvent(
            new TransactionCreatedDomainEvent(transaction.Id, transaction.ProgramId));

        return transaction;
    }
}
