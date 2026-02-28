using System.ComponentModel.DataAnnotations;
using Deep.Accounts.Application.Authentication;
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
using Microsoft.Extensions.Options;

namespace Deep.Accounts.Application.Features.Authentication;

public static class RefreshAccessToken
{
    public sealed record Command([Required] string RefreshToken);

    public sealed record Response(
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiry
    );

    public sealed class Handler(
        AccountsDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtSettings> jwtSettings
    ) : IRequestHandler<Command, Response>
    {
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;

        public async Task<Result<Response>> Handle(Command c, CancellationToken ct = default)
        {
            RefreshToken? existingToken = await context.RefreshTokens.SingleOrDefaultAsync(
                rt => rt.Token == c.RefreshToken,
                ct
            );

            if (existingToken is null || !existingToken.IsActive)
            {
                return AuthErrors.InvalidRefreshToken;
            }

            Account? account = await context
                .Accounts.Include(a => a.Roles)
                .SingleOrDefaultAsync(acct => acct.Id == existingToken.AccountId, ct);

            if (account is null)
            {
                return AuthErrors.InvalidRefreshToken;
            }

            if (!account.IsActive)
            {
                return AuthErrors.AccountInactive;
            }

            // Revoke old token
            existingToken.Revoke();

            // Create new refresh token
            var newRefreshToken = RefreshToken.Create(
                account.Id,
                TimeSpan.FromDays(_jwtSettings.RefreshTokenExpirationDays)
            );
            context.RefreshTokens.Add(newRefreshToken);

            // Generate new access token
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
