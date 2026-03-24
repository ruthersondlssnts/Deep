using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;
using Vast.Transactions.Application.Data;
using Vast.Transactions.Domain.Transaction;
using Microsoft.EntityFrameworkCore;

namespace Vast.Transactions.Application.Features.Transactions;

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
        public async Task<Result<Response>> Handle(
            Query query,
            CancellationToken cancellationToken = default
        )
        {
            Transaction? transaction = await context.Transactions.FirstOrDefaultAsync(
                t => t.Id == query.TransactionId,
                cancellationToken
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
