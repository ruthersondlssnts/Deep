using Vast.Common.Domain;

namespace Vast.Transactions.Domain.Transaction;

public static class TransactionErrors
{
    public static Error NotFound(Guid transactionId) =>
        Error.NotFound(
            "Transactions.NotFound",
            $"The transaction with the identifier {transactionId} was not found"
        );

    public static readonly Error InvalidQuantity = Error.Problem(
        "Transactions.InvalidQuantity",
        "The quantity must be greater than zero"
    );

    public static readonly Error InvalidUnitPrice = Error.Problem(
        "Transactions.InvalidUnitPrice",
        "The unit price must be zero or greater"
    );

    public static readonly Error InvalidProductSku = Error.Problem(
        "Transactions.InvalidProductSku",
        "The product SKU is required"
    );

    public static Error InvalidStatusTransition(TransactionStatus from, TransactionStatus to) =>
        Error.Problem(
            "Transactions.InvalidStatusTransition",
            $"Cannot transition transaction from {from} to {to}"
        );

    public static readonly Error CannotFailCompletedTransaction = Error.Problem(
        "Transactions.CannotFailCompletedTransaction",
        "Cannot fail a completed or refunded transaction"
    );

    public static readonly Error CannotCancelCompletedTransaction = Error.Problem(
        "Transactions.CannotCancelCompletedTransaction",
        "Cannot cancel a completed or refunded transaction"
    );

    public static readonly Error AlreadyCancelled = Error.Problem(
        "Transactions.AlreadyCancelled",
        "The transaction has already been cancelled"
    );

    public static readonly Error CannotRefundNonCompletedTransaction = Error.Problem(
        "Transactions.CannotRefundNonCompletedTransaction",
        "Can only refund a completed transaction"
    );

    public static Error ProgramNotFound(Guid programId) =>
        Error.NotFound(
            "Transactions.ProgramNotFound",
            $"The program with identifier {programId} was not found"
        );

    public static Error CustomerNotFound(Guid customerId) =>
        Error.NotFound(
            "Transactions.CustomerNotFound",
            $"The customer with identifier {customerId} was not found"
        );
}
