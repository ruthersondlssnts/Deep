using Deep.Accounts.Data;
using Deep.Common.Api.ApiResults;
using Deep.Common.Api.Endpoints;
using Deep.Common.Domain;
using Deep.Common.SimpleMediatR;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Deep.Accounts.Features.Accounts
{
    public static class GetAccounts
    {
        public sealed record Query(Role? Role);

        public sealed record Response(
            Guid Id,
            string FirstName,
            string LastName,
            string Email,
            string Role);

        public sealed class Handler(
            AccountsDbContext context)
            : IRequestHandler<Query, IReadOnlyList<Response>>
        {
            public async Task<Result<IReadOnlyList<Response>>> Handle(
                Query request,
                CancellationToken ct)
            {
                var accountsQuery = context.Accounts.AsQueryable();

                if (request.Role.HasValue)
                {
                    accountsQuery = accountsQuery
                        .Where(u => u.Role == request.Role.Value);
                }

                var accounts = await accountsQuery
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .Select(u => new Response(
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.Role.ToString()))
                    .ToListAsync(ct);

                return accounts;
            }
        }
        public sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app) =>
                app.MapGet("/accounts", async (
                    Role? role,
                    IRequestHandler<Query, IReadOnlyList<Response>> handler,
                    CancellationToken ct) =>
                {
                    var result = await handler.Handle(
                        new Query(role), ct);

                    return result.Match(
                        Results.Ok,
                        ApiResults.Problem);
                })
                .WithTags("Accounts");
        }
    }

}
