using Deep.Accounts.Application.Data;
using Deep.Accounts.Domain.Accounts;
using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Deep.Accounts.Application.Features.Accounts;

public static class RegisterAccount
{
    public sealed record Command(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        IReadOnlyCollection<string> Roles
    );

    public sealed record Response(Guid Id);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Roles).NotNull().NotEmpty();

            RuleForEach(x => x.Roles).NotEmpty();

            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);

            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

            RuleFor(x => x.Email).NotEmpty().EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long");
        }
    }

    public sealed class Handler(
        AccountsDbContext context,
        IPasswordHasher<Account> passwordHasher
    ) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command c, CancellationToken ct = default)
        {
            bool emailExists = await context.Accounts.AnyAsync(a => a.Email == c.Email, ct);
            if (emailExists)
            {
                return AuthErrors.EmailAlreadyExists;
            }

            string passwordHash = passwordHasher.HashPassword(null!, c.Password);

            Result<Account> accountResult = Account.Create(
                c.FirstName,
                c.LastName,
                c.Email,
                passwordHash,
                c.Roles
            );

            if (accountResult.IsFailure)
            {
                return accountResult.Error;
            }

            Account account = accountResult.Value;

            foreach (Role role in account.Roles)
            {
                context.Attach(role);
            }

            context.Accounts.Add(account);

            await context.SaveChangesAsync(ct);

            return new Response(account.Id);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPost(
                    "/accounts/register",
                    async (
                        Command command,
                        IRequestHandler<Command, Response> handler,
                        CancellationToken ct
                    ) =>
                    {
                        Result<Response> result = await handler.Handle(command, ct);

                        return result.Match(
                            () => Results.Created($"/accounts/{result.Value.Id}", result.Value),
                            ApiResults.Problem
                        );
                    }
                )
                .WithTags("Accounts");
    }
}
