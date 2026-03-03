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

namespace Deep.Accounts.Application.Features.Passwords;

public static class ResetPassword
{
    public sealed record Command(
        [Required] string ResetToken,
        [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
            string NewPassword
    );

    public sealed record Response(bool Success);

    public sealed class Handler(AccountsDbContext context, IPasswordHasher<Account> passwordHasher)
        : IRequestHandler<Command, Response>
    {
        private const int PasswordHistoryLimit = 5;

        public async Task<Result<Response>> Handle(Command c, CancellationToken ct = default)
        {
            PasswordResetToken? resetToken = await context.PasswordResetTokens.SingleOrDefaultAsync(
                prt => prt.Token == c.ResetToken,
                ct
            );

            if (resetToken is null || !resetToken.IsValid)
            {
                return AuthErrors.InvalidResetToken;
            }

            Account? account = await context
                .Accounts.Include(a => a.Roles)
                .SingleOrDefaultAsync(acct => acct.Id == resetToken.AccountId, ct);

            if (account is null)
            {
                return AuthErrors.InvalidResetToken;
            }

            if (!account.IsActive)
            {
                return AuthErrors.AccountInactive;
            }

            List<PasswordHistory> passwordHistory = await context
                .PasswordHistories.Where(ph => ph.AccountId == account.Id)
                .OrderByDescending(ph => ph.ChangedAtUtc)
                .Take(PasswordHistoryLimit)
                .ToListAsync(ct);

            if (
                passwordHasher.VerifyHashedPassword(account, account.PasswordHash, c.NewPassword)
                != PasswordVerificationResult.Failed
            )
            {
                return AuthErrors.PasswordRecentlyUsed;
            }

            foreach (PasswordHistory history in passwordHistory)
            {
                if (
                    passwordHasher.VerifyHashedPassword(
                        account,
                        history.PasswordHash,
                        c.NewPassword
                    ) != PasswordVerificationResult.Failed
                )
                {
                    return AuthErrors.PasswordRecentlyUsed;
                }
            }

            var historyEntry = PasswordHistory.Create(account.Id, account.PasswordHash);
            context.PasswordHistories.Add(historyEntry);

            List<PasswordHistory> toDelete = await context
                .PasswordHistories.Where(ph => ph.AccountId == account.Id)
                .OrderByDescending(ph => ph.ChangedAtUtc)
                .Skip(PasswordHistoryLimit)
                .ToListAsync(ct);

            context.PasswordHistories.RemoveRange(toDelete);

            string newPasswordHash = passwordHasher.HashPassword(account, c.NewPassword);
            account.UpdatePassword(newPasswordHash);

            List<RefreshToken> activeTokens = await context
                .RefreshTokens.Where(rt => rt.AccountId == account.Id && rt.RevokedAtUtc == null)
                .ToListAsync(ct);

            foreach (RefreshToken token in activeTokens)
            {
                token.Revoke();
            }

            resetToken.MarkAsUsed();

            await context.SaveChangesAsync(ct);

            return new Response(true);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPost(
                    "/accounts/reset-password",
                    async (
                        Command command,
                        IRequestHandler<Command, Response> handler,
                        CancellationToken ct
                    ) =>
                    {
                        Result<Response> result = await handler.Handle(command, ct);

                        return result.Match(() => Results.Ok(result.Value), ApiResults.Problem);
                    }
                )
                .WithTags("Accounts");
    }
}
