using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Transactions.Application.Data;
using Deep.Transactions.Domain.Transaction;
using Microsoft.EntityFrameworkCore;

namespace Deep.Transactions.Application.Features.Transactions;

public static class UpdateTransactionStatus
{
    public sealed record Command(
        Guid TransactionId,
        TransactionStatus Status,
        string? PaymentReference = null,
        string? FailureReason = null
    );

    public sealed record Response(bool Updated);

    public sealed class Handler(TransactionsDbContext context) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken ct = default)
        {
            Transaction? transaction = await context.Transactions.FirstOrDefaultAsync(
                t => t.Id == command.TransactionId,
                ct
            );

            if (transaction is null)
            {
                return new Response(false);
            }

            Result<bool> transitionResult = command.Status switch
            {
                TransactionStatus.Completed => HandleComplete(
                    transaction,
                    command.PaymentReference ?? string.Empty
                ),
                TransactionStatus.Failed => HandleFail(
                    transaction,
                    command.FailureReason ?? string.Empty
                ),
                _ => true,
            };

            if (transitionResult.IsFailure)
            {
                return transitionResult.Error;
            }

            if (!transitionResult.Value)
            {
                return new Response(false);
            }

            await context.SaveChangesAsync(ct);

            return new Response(true);
        }

        private static Result<bool> HandleComplete(Transaction transaction, string paymentReference)
        {
            if (transaction.Status == TransactionStatus.Completed)
            {
                return false;
            }

            Result transitionResult = transaction.Complete(paymentReference);

            if (transitionResult.IsFailure)
            {
                return transitionResult.Error;
            }

            return true;
        }

        private static Result<bool> HandleFail(Transaction transaction, string reason)
        {
            if (transaction.Status == TransactionStatus.Failed)
            {
                return false;
            }

            Result failResult = transaction.Fail(reason);

            if (failResult.IsFailure)
            {
                return failResult.Error;
            }

            return true;
        }
    }
}
