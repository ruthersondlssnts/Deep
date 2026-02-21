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
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Deep.Accounts.Application.Features.Accounts;

public static class RefreshAccessToken
{
    public sealed record Command(string RefreshToken);

    public sealed record Response(
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiry
    );

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }

    public sealed class Handler(
        AccountsDbContext context,
        IAccountRepository accountRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtSettings> jwtSettings
    ) : IRequestHandler<Command, Response>
    {
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;

        public async Task<Result<Response>> Handle(Command c, CancellationToken ct)
        {
            Account? account = await accountRepository.GetByRefreshTokenAsync(c.RefreshToken, ct);

            if (account is null)
            {
                return AuthErrors.InvalidRefreshToken;
            }

            if (!account.IsActive)
            {
                return AuthErrors.AccountInactive;
            }

            RefreshToken? existingToken = account.GetActiveRefreshToken(c.RefreshToken);

            if (existingToken is null)
            {
                return AuthErrors.InvalidRefreshToken;
            }

            account.RevokeRefreshToken(existingToken);

            RefreshToken newRefreshToken = account.AddRefreshToken(
                TimeSpan.FromDays(_jwtSettings.RefreshTokenExpirationDays)
            );

            string accessToken = jwtTokenGenerator.GenerateAccessToken(account);

            await context.SaveChangesAsync(ct);

            return new Response(
                accessToken,
                newRefreshToken.Token,
                DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
            );
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPost(
                    "/accounts/refresh-token",
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
