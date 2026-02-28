using System.ComponentModel.DataAnnotations;
using Deep.Accounts.Application.Data;
using Deep.Accounts.Domain.Accounts;
using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Deep.Accounts.Application.Features.Accounts;

public sealed record RegisterAccountCommand(
    [Required] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        string Password,
    [Required, MinLength(1)] IReadOnlyCollection<string> Roles
);

public sealed record RegisterAccountResponse(Guid Id);

public sealed class RegisterAccountHandler
    : IRequestHandler<RegisterAccountCommand, RegisterAccountResponse>
{
    private readonly AccountsDbContext _context;
    private readonly IPasswordHasher<Account> _passwordHasher;

    public RegisterAccountHandler(
        AccountsDbContext context,
        IPasswordHasher<Account> passwordHasher
    )
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegisterAccountResponse>> Handle(
        RegisterAccountCommand c,
        CancellationToken ct = default
    )
    {
        bool emailExists = await _context.Accounts.AnyAsync(a => a.Email == c.Email, ct);
        if (emailExists)
        {
            return AuthErrors.EmailAlreadyExists;
        }

        string passwordHash = _passwordHasher.HashPassword(null!, c.Password);

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
            _context.Attach(role);
        }

        _context.Accounts.Add(account);

        await _context.SaveChangesAsync(ct);

        return new RegisterAccountResponse(account.Id);
    }
}

public sealed class RegisterAccountEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost(
                "/accounts/register",
                async (
                    RegisterAccountCommand command,
                    IRequestHandler<RegisterAccountCommand, RegisterAccountResponse> handler,
                    CancellationToken ct
                ) =>
                {
                    Result<RegisterAccountResponse> result = await handler.Handle(command, ct);

                    return result.Match(
                        () => Results.Created($"/accounts/{result.Value.Id}", result.Value),
                        ApiResults.Problem
                    );
                }
            )
            .WithTags("Accounts");
}
