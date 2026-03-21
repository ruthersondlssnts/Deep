using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.Features.Users;

public static class GetUsers
{
    public sealed record Query(string? Role);

    public sealed record Response(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        IReadOnlyList<string> Roles
    );

    public sealed class Handler(ProgramsDbContext context)
        : IRequestHandler<Query, IReadOnlyList<Response>>
    {
        public async Task<Result<IReadOnlyList<Response>>> Handle(
            Query request,
            CancellationToken cancellationToken = default
        )
        {
            IQueryable<User> accountsQuery = context.Users.Include(a => a.Roles).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                accountsQuery = accountsQuery.Where(a => a.Roles.Any(r => r.Name == request.Role));
            }

            List<Response> accounts = await accountsQuery
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.FirstName)
                .Select(a => new Response(
                    a.Id,
                    a.FirstName,
                    a.LastName,
                    a.Email,
                    a.Roles.Select(r => r.Name).ToList()
                ))
                .ToListAsync(cancellationToken);

            return accounts;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapGet(
                    "/users",
                    async (
                        string? role,
                        IRequestHandler<Query, IReadOnlyList<Response>> handler,
                        CancellationToken cancellationToken
                    ) =>
                    {
                        Result<IReadOnlyList<Response>> result = await handler.Handle(
                            new Query(role),
                            cancellationToken
                        );

                        return result.Match(Results.Ok, ApiResults.Problem);
                    }
                )
                .WithTags("Users");
    }
}
