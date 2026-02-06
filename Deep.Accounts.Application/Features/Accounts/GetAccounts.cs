using Deep.Accounts.Application.Data;
using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Deep.Accounts.Application.Features.Accounts;

public static class GetAccounts
{
    public sealed record Query(string? Role);

    public sealed record Response(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        IReadOnlyCollection<string> Roles);

    public sealed class Handler(
        AccountsDbContext context)
        : IRequestHandler<Query, IReadOnlyCollection<Response>>
    {
        public async Task<Result<IReadOnlyCollection<Response>>> Handle(
            Query request,
            CancellationToken ct)
        {
            var accountsQuery = context.Accounts
                .Include(a => a.Roles)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                accountsQuery = accountsQuery
                    .Where(a => a.Roles.Any(r => r.Name == request.Role));
            }

            var accounts = await accountsQuery
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.FirstName)
                .Select(a => new Response(
                    a.Id,
                    a.FirstName,
                    a.LastName,
                    a.Email,
                    a.Roles.Select(r => r.Name).ToList()))
                .ToListAsync(ct);

            return accounts;
        }
    }
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapGet("/accounts", async (
                string? role,
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
