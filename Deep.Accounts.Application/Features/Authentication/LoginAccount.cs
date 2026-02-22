using Deep.Accounts.Application.Authentication;
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
using Microsoft.Extensions.Options;

namespace Deep.Accounts.Application.Features.Authentication;

public static class LoginAccount
{
    public sealed record Command(string Email, string Password);

    public sealed record Response(
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiry
    );

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public sealed class Handler(
        AccountsDbContext context,
        IAccountRepository accountRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher<Account> passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtSettings> jwtSettings
    ) : IRequestHandler<Command, Response>
    {
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;

        public async Task<Result<Response>> Handle(Command c, CancellationToken ct)
        {
            Account? account = await accountRepository.GetByEmailAsync(c.Email, ct);

            if (account is null)
            {
                return AuthErrors.InvalidCredentials;
            }

            if (!account.IsActive)
            {
                return AuthErrors.AccountInactive;
            }

            PasswordVerificationResult verificationResult = passwordHasher.VerifyHashedPassword(
                account,
                account.PasswordHash,
                c.Password
            );

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return AuthErrors.InvalidCredentials;
            }

            string accessToken = jwtTokenGenerator.GenerateAccessToken(account);

            var refreshToken = RefreshToken.Create(
                account.Id,
                TimeSpan.FromDays(_jwtSettings.RefreshTokenExpirationDays)
            );
            refreshTokenRepository.Insert(refreshToken);

            await context.SaveChangesAsync(ct);

            return new Response(
                accessToken,
                refreshToken.Token,
                DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
            );
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPost(
                    "/accounts/login",
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
