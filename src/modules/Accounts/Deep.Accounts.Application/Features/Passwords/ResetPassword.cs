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
        [property: Required] string ResetToken,
        [property: Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters long")] string NewPassword
    );

    public sealed record Response(bool Success);

    public sealed class Handler(AccountsDbContext context, IPasswordHasher<Account> passwordHasher)
        : IRequestHandler<Command, Response>
    {
        private const int PasswordHistoryLimit = 5;

        public async Task<Result<Response>> Handle(Command c, CancellationToken ct = default)
        {
            // Query reset token directly
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

            // Check password history - prevent reuse of last 5 passwords
            List<PasswordHistory> passwordHistory = await context
                .PasswordHistories.Where(ph => ph.AccountId == account.Id)
                .OrderByDescending(ph => ph.ChangedAtUtc)
                .Take(PasswordHistoryLimit)
                .ToListAsync(ct);

            // Check if new password matches current password
            if (
                passwordHasher.VerifyHashedPassword(account, account.PasswordHash, c.NewPassword)
                != PasswordVerificationResult.Failed
            )
            {
                return AuthErrors.PasswordRecentlyUsed;
            }

            // Check against password history
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

            // Save current password to history
            var historyEntry = PasswordHistory.Create(account.Id, account.PasswordHash);
            context.PasswordHistories.Add(historyEntry);

            // Delete oldest history beyond limit
            List<PasswordHistory> toDelete = await context
                .PasswordHistories.Where(ph => ph.AccountId == account.Id)
                .OrderByDescending(ph => ph.ChangedAtUtc)
                .Skip(PasswordHistoryLimit)
                .ToListAsync(ct);

            context.PasswordHistories.RemoveRange(toDelete);

            // Update password
            string newPasswordHash = passwordHasher.HashPassword(account, c.NewPassword);
            account.UpdatePassword(newPasswordHash);

            // Revoke all refresh tokens
            List<RefreshToken> activeTokens = await context
                .RefreshTokens.Where(rt => rt.AccountId == account.Id && rt.RevokedAtUtc == null)
                .ToListAsync(ct);

            foreach (RefreshToken token in activeTokens)
            {
                token.Revoke();
            }

            // Mark reset token as used
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
