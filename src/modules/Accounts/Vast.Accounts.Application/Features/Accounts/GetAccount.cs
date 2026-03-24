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

namespace Vast.Accounts.Application.Features.Accounts;

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
        public async Task<Result<Response>> Handle(
            Query query,
            CancellationToken cancellationToken = default
        )
        {
            Response? user = await context
                .Accounts.Where(u => u.Id == query.Id)
                .Include(u => u.Roles)
                .Select(u => new Response(u.Id, u.FirstName, u.LastName, u.Email, u.Roles))
                .FirstOrDefaultAsync(cancellationToken);

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
                        CancellationToken cancellationToken
                    ) =>
                    {
                        Result<Response> result = await handler.Handle(
                            new Query(id),
                            cancellationToken
                        );

                        return result.Match(Results.Ok, ApiResults.Problem);
                    }
                )
                .WithTags("Accounts");
    }
}
