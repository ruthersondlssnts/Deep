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

namespace Deep.Accounts.Application.Features.Passwords;

public static class ForgotPassword
{
    public sealed record Command(string Email);

    public sealed record Response(string ResetToken, DateTime ExpiresAtUtc);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator() => RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }

    public sealed class Handler(
        AccountsDbContext context,
        IAccountRepository accountRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository
    ) : IRequestHandler<Command, Response>
    {
        private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromMinutes(15);

        public async Task<Result<Response>> Handle(Command c, CancellationToken ct)
        {
            Account? account = await accountRepository.GetByEmailAsync(c.Email, ct);

            if (account is null)
            {
                return AccountErrors.NotFound(Guid.Empty);
            }

            if (!account.IsActive)
            {
                return AuthErrors.AccountInactive;
            }

            passwordResetTokenRepository.InvalidateAllForAccount(account.Id);

            var resetToken = PasswordResetToken.Create(account.Id, ResetTokenLifetime);
            passwordResetTokenRepository.Insert(resetToken);

            await context.SaveChangesAsync(ct);

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
