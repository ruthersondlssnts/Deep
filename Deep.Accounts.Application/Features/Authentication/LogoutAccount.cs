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

namespace Deep.Accounts.Application.Features.Authentication;

public static class LogoutAccount
{
    public sealed record Command(string RefreshToken);

    public sealed record Response(bool Success);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator() => RuleFor(x => x.RefreshToken).NotEmpty();
    }

    public sealed class Handler(
        AccountsDbContext context,
        IRefreshTokenRepository refreshTokenRepository
    ) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command c, CancellationToken ct)
        {
            RefreshToken? existingToken = await refreshTokenRepository.GetByTokenAsync(
                c.RefreshToken,
                ct
            );

            if (existingToken is null)
            {
                return AuthErrors.InvalidRefreshToken;
            }

            existingToken.Revoke();

            await context.SaveChangesAsync(ct);

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
