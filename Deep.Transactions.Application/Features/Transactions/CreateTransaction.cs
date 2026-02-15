using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Transactions.Application.Data;
using Deep.Transactions.Domain.Customer;
using Deep.Transactions.Domain.Transaction;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Deep.Transactions.Application.Features.Transactions;

public static class CreateTransaction
{
    public sealed record Command(Guid ProgramId, string CustomerEmail, string CustomerFullName);

    public sealed record Response(Guid TransactionId, Guid? CustomerId);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.CustomerEmail).EmailAddress().NotEmpty();

            RuleFor(x => x.CustomerFullName).NotEmpty();
        }
    }

    public sealed class Handler(
        TransactionsDbContext context,
        ICustomerRepository customerRepository,
        ITransactionRepository transactionRepository
    ) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken ct = default)
        {
            Customer? customer = await context.Customers.FirstOrDefaultAsync(
                c => c.Email == command.CustomerEmail,
                ct
            );

            if (customer is null)
            {
                customer = Customer.Create(command.CustomerFullName, command.CustomerEmail).Value;
                customerRepository.Insert(customer);
            }

            Transaction transaction = Transaction.Create(command.ProgramId, customer.Id).Value;
            transactionRepository.Insert(transaction);

            await context.SaveChangesAsync(ct);

            return new Response(transaction.Id, customer.Id);
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
                        CancellationToken ct
                    ) =>
                    {
                        Result<Response> result = await handler.Handle(command, ct);

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
