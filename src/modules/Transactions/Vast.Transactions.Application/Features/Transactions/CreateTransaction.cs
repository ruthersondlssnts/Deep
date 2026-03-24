using System.ComponentModel.DataAnnotations;
using Vast.Common.Application.Api.ApiResults;
using Vast.Common.Application.Api.Endpoints;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;
using Vast.Transactions.Application.Data;
using Vast.Transactions.Domain.Customer;
using Vast.Transactions.Domain.Transaction;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Vast.Transactions.Application.Features.Transactions;

public static class CreateTransaction
{
    public sealed record Command(
        [Required] Guid ProgramId,
        [Required] string ProductSku,
        [Required] string ProductName,
        [Required, Range(1, int.MaxValue)] int Quantity,
        [Required, Range(0, double.MaxValue)] decimal UnitPrice,
        [Required, EmailAddress] string CustomerEmail,
        [Required] string CustomerFullName
    );

    public sealed record Response(Guid TransactionId, Guid CustomerId);

    public sealed class Handler(TransactionsDbContext context) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(
            Command command,
            CancellationToken cancellationToken = default
        )
        {
            Customer? customer = await context.Customers.FirstOrDefaultAsync(
                c => c.Email == command.CustomerEmail,
                cancellationToken
            );

            if (customer is null)
            {
                customer = Customer.Create(command.CustomerFullName, command.CustomerEmail).Value;
                context.Customers.Add(customer);
            }

            Result<Transaction> transactionResult = Transaction.Create(
                command.ProgramId,
                customer.Id,
                command.ProductSku,
                command.ProductName,
                command.Quantity,
                command.UnitPrice
            );

            if (transactionResult.IsFailure)
            {
                return transactionResult.Error;
            }

            context.Transactions.Add(transactionResult.Value);
            await context.SaveChangesAsync(cancellationToken);

            return new Response(transactionResult.Value.Id, customer.Id);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPost(
                    "/transactions",
                    async (
                        Command command,
                        IRequestHandler<Command, Response> handler,
                        CancellationToken cancellationToken
                    ) =>
                    {
                        Result<Response> result = await handler.Handle(command, cancellationToken);

                        return result.Match(
                            () =>
                                Results.Created(
                                    $"/transactions/{result.Value.TransactionId}",
                                    result.Value
                                ),
                            ApiResults.Problem
                        );
                    }
                )
                .WithTags("Transactions");
    }
}
