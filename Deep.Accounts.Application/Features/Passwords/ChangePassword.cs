using Deep.Accounts.Application.Data;
using Deep.Accounts.Domain.Accounts;
using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.Authentication;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Deep.Accounts.Application.Features.Passwords;

public static class ChangePassword
{
    public sealed record Command(string CurrentPassword, string NewPassword);

    public sealed record Response(bool Success);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long");
        }
    }

    public sealed class Handler(
        AccountsDbContext context,
        IPasswordHasher<Account> passwordHasher,
        IHttpContextAccessor httpContextAccessor
    ) : IRequestHandler<Command, Response>
    {
        private const int PasswordHistoryLimit = 5;

        public async Task<Result<Response>> Handle(Command c, CancellationToken ct = default)
        {
            Guid accountId = httpContextAccessor.HttpContext!.User.GetUserId();

            Account? account = await context
                .Accounts.Include(a => a.Roles)
                .SingleOrDefaultAsync(acct => acct.Id == accountId, ct);

            if (account is null)
            {
                return AccountErrors.NotFound(accountId);
            }

            PasswordVerificationResult verificationResult = passwordHasher.VerifyHashedPassword(
                account,
                account.PasswordHash,
                c.CurrentPassword
            );

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return AuthErrors.CurrentPasswordIncorrect;
            }

            List<PasswordHistory> passwordHistory = await context
                .PasswordHistories.Where(ph => ph.AccountId == accountId)
                .OrderByDescending(ph => ph.ChangedAtUtc)
                .Take(PasswordHistoryLimit)
                .ToListAsync(ct);

            string newPasswordHash = passwordHasher.HashPassword(account, c.NewPassword);

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

            var historyEntry = PasswordHistory.Create(accountId, account.PasswordHash);
            context.PasswordHistories.Add(historyEntry);

            var toDelete = await context
                .PasswordHistories.Where(ph => ph.AccountId == accountId)
                .OrderByDescending(ph => ph.ChangedAtUtc)
                .Skip(PasswordHistoryLimit)
                .ToListAsync(ct);

            context.PasswordHistories.RemoveRange(toDelete);

            account.UpdatePassword(newPasswordHash);

            var activeTokens = await context
                .RefreshTokens.Where(rt => rt.AccountId == accountId && rt.RevokedAtUtc == null)
                .ToListAsync(ct);

            foreach (RefreshToken token in activeTokens)
            {
                token.Revoke();
            }

            await context.SaveChangesAsync(ct);

            return new Response(true);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPost(
                    "/accounts/change-password",
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
                .RequireAuthorization()
                .WithTags("Accounts");
    }
}
