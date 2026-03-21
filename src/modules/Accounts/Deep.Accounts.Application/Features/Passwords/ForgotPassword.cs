using System.ComponentModel.DataAnnotations;
using Deep.Accounts.Application.Data;
using Deep.Accounts.Domain.Accounts;
using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Deep.Accounts.Application.Features.Passwords;

public static class ForgotPassword
{
    public sealed record Command([Required, EmailAddress] string Email);

    public sealed record Response(string ResetToken, DateTime ExpiresAtUtc);

    public sealed class Handler(AccountsDbContext context) : IRequestHandler<Command, Response>
    {
        private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromMinutes(15);

        public async Task<Result<Response>> Handle(
            Command c,
            CancellationToken cancellationToken = default
        )
        {
            Account? account = await context
                .Accounts.Include(a => a.Roles)
                .SingleOrDefaultAsync(acct => acct.Email == c.Email, cancellationToken);

            if (account is null)
            {
                return AccountErrors.NotFound(Guid.Empty);
            }

            if (!account.IsActive)
            {
                return AuthErrors.AccountInactive;
            }

            List<PasswordResetToken> activeTokens = await context
                .PasswordResetTokens.Where(prt =>
                    prt.AccountId == account.Id && prt.UsedAtUtc == null
                )
                .ToListAsync(cancellationToken);

            foreach (PasswordResetToken token in activeTokens)
            {
                token.MarkAsUsed();
            }

            var resetToken = PasswordResetToken.Create(account.Id, ResetTokenLifetime);
            context.PasswordResetTokens.Add(resetToken);

            await context.SaveChangesAsync(cancellationToken);

            // In production, you would send this token via email
            return new Response(resetToken.Token, resetToken.ExpiryDateUtc);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPost(
                    "/accounts/forgot-password",
                    async (
                        Command command,
                        IRequestHandler<Command, Response> handler,
                        CancellationToken cancellationToken
                    ) =>
                    {
                        Result<Response> result = await handler.Handle(command, cancellationToken);

                        return result.Match(() => Results.Ok(result.Value), ApiResults.Problem);
                    }
                )
                .WithTags("Accounts");
    }
}
