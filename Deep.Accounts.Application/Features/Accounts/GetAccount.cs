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

namespace Deep.Accounts.Application.Features.Accounts;

public static class GetAccount
{
    public sealed record Query(Guid Id);

    public sealed record Response(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        IReadOnlyCollection<Role> Roles
    );

    public sealed class Handler(AccountsDbContext context) : IRequestHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken ct = default)
        {
            var user = await context
                .Accounts.Where(u => u.Id == query.Id)
                .Include(u => u.Roles)
                .Select(u => new Response(u.Id, u.FirstName, u.LastName, u.Email, u.Roles))
                .FirstOrDefaultAsync(ct);

            return user is null ? AccountErrors.NotFound(query.Id) : user;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapGet(
                    "/accounts/{id:guid}",
                    async (
                        Guid id,
                        IRequestHandler<Query, Response> handler,
                        CancellationToken ct
                    ) =>
                    {
                        var result = await handler.Handle(new Query(id), ct);

                        return result.Match(Results.Ok, ApiResults.Problem);
                    }
                )
                .WithTags("Accounts");
    }
}
