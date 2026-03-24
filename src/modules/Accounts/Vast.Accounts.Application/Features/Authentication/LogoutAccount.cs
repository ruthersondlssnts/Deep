using System.ComponentModel.DataAnnotations;
using Vast.Accounts.Application.Data;
using Vast.Accounts.Domain.Accounts;
using Vast.Common.Application.Api.ApiResults;
using Vast.Common.Application.Api.Endpoints;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Vast.Accounts.Application.Features.Authentication;

public static class LogoutAccount
{
    public sealed record Command([Required] string RefreshToken);

    public sealed record Response(bool Success);

    public sealed class Handler(AccountsDbContext context) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(
            Command c,
            CancellationToken cancellationToken = default
        )
        {
            RefreshToken? existingToken = await context.RefreshTokens.SingleOrDefaultAsync(
                rt => rt.Token == c.RefreshToken,
                cancellationToken
            );

            if (existingToken is null)
            {
                return AuthErrors.InvalidRefreshToken;
            }

            existingToken.Revoke();

            await context.SaveChangesAsync(cancellationToken);

            return new Response(true);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPost(
                    "/accounts/logout",
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
