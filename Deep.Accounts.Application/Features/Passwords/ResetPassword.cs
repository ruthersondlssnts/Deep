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

namespace Deep.Accounts.Application.Features.Passwords;

public static class ResetPassword
{
    public sealed record Command(string ResetToken, string NewPassword);

    public sealed record Response(bool Success);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ResetToken).NotEmpty();
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long");
        }
    }

    public sealed class Handler(
        AccountsDbContext context,
        IAccountRepository accountRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHistoryRepository passwordHistoryRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IPasswordHasher<Account> passwordHasher
    ) : IRequestHandler<Command, Response>
    {
        private const int PasswordHistoryLimit = 5;

        public async Task<Result<Response>> Handle(Command c, CancellationToken ct = default)
        {
            // Query reset token directly
            PasswordResetToken? resetToken = await passwordResetTokenRepository.GetByTokenAsync(
                c.ResetToken,
                ct
            );

            if (resetToken is null || !resetToken.IsValid)
            {
                return AuthErrors.InvalidResetToken;
            }

            Account? account = await accountRepository.GetAsync(resetToken.AccountId, ct);

            if (account is null)
            {
                return AuthErrors.InvalidResetToken;
            }

            if (!account.IsActive)
            {
                return AuthErrors.AccountInactive;
            }

            // Check password history - prevent reuse of last 5 passwords
            IReadOnlyCollection<PasswordHistory> passwordHistory = await passwordHistoryRepository.GetLastNByAccountIdAsync(
                account.Id,
                PasswordHistoryLimit,
                ct
            );

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
            passwordHistoryRepository.Insert(historyEntry);

            // Delete oldest history beyond limit
            passwordHistoryRepository.DeleteOldestBeyondLimit(account.Id, PasswordHistoryLimit);

            // Update password
            string newPasswordHash = passwordHasher.HashPassword(account, c.NewPassword);
            account.UpdatePassword(newPasswordHash);

            // Revoke all refresh tokens
            refreshTokenRepository.RevokeAllForAccount(account.Id);

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
