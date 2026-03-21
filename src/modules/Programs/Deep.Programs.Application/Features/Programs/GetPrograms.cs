using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.Features.Programs;

public static class GetPrograms
{
    public sealed record Query;

    public sealed record Response(
        Guid Id,
        string Name,
        string Description,
        DateTime StartsAtUtc,
        DateTime EndsAtUtc,
        IReadOnlyCollection<string> Products
    );

    public sealed class Handler(ProgramsDbContext context)
        : IRequestHandler<Query, IReadOnlyList<Response>>
    {
        public async Task<Result<IReadOnlyList<Response>>> Handle(
            Query query,
            CancellationToken cancellationToken = default
        )
        {
            List<Response> programs = await context
                .Programs.OrderBy(p => p.StartsAtUtc)
                .Select(p => new Response(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.StartsAtUtc,
                    p.EndsAtUtc,
                    p.Products.Select(pp => pp.ProductName).ToList()
                ))
                .ToListAsync(cancellationToken);

            return programs;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapGet(
                    "/programs",
                    async (
                        IRequestHandler<Query, IReadOnlyList<Response>> handler,
                        CancellationToken cancellationToken
                    ) =>
                    {
                        Result<IReadOnlyList<Response>> result = await handler.Handle(
                            new Query(),
                            cancellationToken
                        );

                        return result.Match(Results.Ok, ApiResults.Problem);
                    }
                )
                .WithTags("Programs");
    }
}
