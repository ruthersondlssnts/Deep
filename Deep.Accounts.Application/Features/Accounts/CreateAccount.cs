using Deep.Accounts.Application.Data;
using Deep.Accounts.Domain.Accounts;
using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Deep.Accounts.Application.Features.Accounts;

public static class CreateAccount
{
    public sealed record Command(
        string FirstName,
        string LastName,
        string Email,
        Role Role);

    public sealed record Response(Guid Id);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Role).IsInEnum();

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty();
        }
    }

    public sealed class Handler(AccountsDbContext context)
        : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(
            Command c,
            CancellationToken ct)
        {
            var account = Account.Create(
                c.FirstName,
                c.LastName,
                c.Email,
                c.Role);

            context.Accounts.Add(account);
            await context.SaveChangesAsync(ct);

            return new Response(account.Id);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPost("/accounts/register", async (
                Command command,
                IRequestHandler<Command, Response> handler,
                CancellationToken ct) =>
            {
                var result = await handler.Handle(command, ct);

                return result.Match(
                    () => Results.Created(
                        $"/accounts/{result.Value.Id}",
                        result.Value),
                    ApiResults.Problem);
            })
            .WithTags("Accounts");
    }
}
