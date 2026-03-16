using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Transactions.Application.Data;
using Deep.Transactions.Domain.Transaction;
using Microsoft.EntityFrameworkCore;

namespace Deep.Transactions.Application.Features.Transactions;

public static class GetTransaction
{
    public sealed record Query(Guid TransactionId);

    public sealed record Response(
        Guid Id,
        Guid ProgramId,
        Guid CustomerId,
        string ProductSku,
        int Quantity,
        decimal TotalAmount
    );

    public sealed class Handler(TransactionsDbContext context) : IRequestHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken ct = default)
        {
            Transaction? transaction = await context.Transactions.FirstOrDefaultAsync(
                t => t.Id == query.TransactionId,
                ct
            );

            if (transaction is null)
            {
                return TransactionErrors.NotFound(query.TransactionId);
            }

            return new Response(
                transaction.Id,
                transaction.ProgramId,
                transaction.CustomerId,
                transaction.ProductSku,
                transaction.Quantity,
                transaction.TotalAmount
            );
        }
    }
}
