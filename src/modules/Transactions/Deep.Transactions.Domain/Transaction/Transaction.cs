using Deep.Common.Domain;
using Deep.Common.Domain.Auditing;

namespace Deep.Transactions.Domain.Transaction;

public enum TransactionStatus
{
    Pending,
    StockReserved,
    PaymentProcessing,
    Completed,
    Failed,
    Cancelled,
    Refunded
}

[Auditable]
public sealed class Transaction : Entity
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ProgramId { get; private set; }
    public string ProductSku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalAmount { get; private set; }
    public TransactionStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime? RefundedAtUtc { get; private set; }
    public string? PaymentReference { get; private set; }
    public string? RefundReference { get; private set; }

    private Transaction() { }

    public static Result<Transaction> Create(
        Guid programId,
        Guid customerId,
        string productSku,
        string productName,
        int quantity,
        decimal unitPrice)
    {
        if (quantity <= 0)
        {
            return TransactionErrors.InvalidQuantity;
        }

        if (unitPrice < 0)
        {
            return TransactionErrors.InvalidUnitPrice;
        }

        if (string.IsNullOrWhiteSpace(productSku))
        {
            return TransactionErrors.InvalidProductSku;
        }

        var transaction = new Transaction
        {
            Id = Guid.CreateVersion7(),
            CustomerId = customerId,
            ProgramId = programId,
            ProductSku = productSku.Trim().ToUpperInvariant(),
            ProductName = productName.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalAmount = quantity * unitPrice,
            Status = TransactionStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        transaction.RaiseDomainEvent(
            new TransactionCreatedDomainEvent(
                transaction.Id,
                transaction.ProgramId,
                transaction.ProductSku,
                transaction.Quantity,
                transaction.TotalAmount)
        );

        return transaction;
    }

    public Result MarkStockReserved()
    {
        if (Status != TransactionStatus.Pending)
        {
            return TransactionErrors.InvalidStatusTransition(Status, TransactionStatus.StockReserved);
        }

        Status = TransactionStatus.StockReserved;
        RaiseDomainEvent(new TransactionStockReservedDomainEvent(Id, ProgramId, ProductSku, Quantity));
        return Result.Success();
    }

    public Result MarkPaymentProcessing()
    {
        if (Status != TransactionStatus.StockReserved)
        {
            return TransactionErrors.InvalidStatusTransition(Status, TransactionStatus.PaymentProcessing);
        }

        Status = TransactionStatus.PaymentProcessing;
        return Result.Success();
    }

    public Result Complete(string paymentReference)
    {
        if (Status != TransactionStatus.PaymentProcessing && Status != TransactionStatus.StockReserved)
        {
            return TransactionErrors.InvalidStatusTransition(Status, TransactionStatus.Completed);
        }

        Status = TransactionStatus.Completed;
        PaymentReference = paymentReference;
        CompletedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new TransactionCompletedDomainEvent(Id, ProgramId, ProductSku, Quantity, TotalAmount));
        return Result.Success();
    }

    public Result Fail(string reason)
    {
        if (Status == TransactionStatus.Completed || Status == TransactionStatus.Refunded)
        {
            return TransactionErrors.CannotFailCompletedTransaction;
        }

        Status = TransactionStatus.Failed;
        FailureReason = reason;

        RaiseDomainEvent(new TransactionFailedDomainEvent(Id, ProgramId, ProductSku, Quantity, reason));
        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (Status == TransactionStatus.Completed || Status == TransactionStatus.Refunded)
        {
            return TransactionErrors.CannotCancelCompletedTransaction;
        }

        if (Status == TransactionStatus.Cancelled)
        {
            return TransactionErrors.AlreadyCancelled;
        }

        Status = TransactionStatus.Cancelled;
        FailureReason = reason;
        CancelledAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new TransactionCancelledDomainEvent(Id, ProgramId, ProductSku, Quantity, reason));
        return Result.Success();
    }

    public Result Refund(string refundReference)
    {
        if (Status != TransactionStatus.Completed)
        {
            return TransactionErrors.CannotRefundNonCompletedTransaction;
        }

        Status = TransactionStatus.Refunded;
        RefundReference = refundReference;
        RefundedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new TransactionRefundedDomainEvent(Id, ProgramId, ProductSku, Quantity, TotalAmount, refundReference));
        return Result.Success();
    }
}
