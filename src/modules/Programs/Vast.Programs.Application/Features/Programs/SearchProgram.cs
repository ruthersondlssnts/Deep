using Vast.Common.Application.Api.ApiResults;
using Vast.Common.Application.Api.Endpoints;
using Vast.Common.Application.Pagination;
using Vast.Common.Domain;
using Vast.Programs.Application.Data;
using Vast.Programs.Domain.Programs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Vast.Programs.Application.Features.Programs;

public static class SearchProgram
{
    public sealed record Query(
        int Page,
        int PageSize,
        string? SearchTerm,
        ProgramStatus? Status,
        DateTime? StartsAfter,
        DateTime? StartsBefore
    ) : PaginatedQuery<IReadOnlyList<Response>>(Page, PageSize);

    public sealed record Response(
        Guid Id,
        string Name,
        string Description,
        DateTime StartsAtUtc,
        DateTime EndsAtUtc,
        ProgramStatus ProgramStatus,
        Guid OwnerId,
        string OwnerName,
        IReadOnlyList<string> Products
    );

    public sealed class Handler(ProgramsDbContext context)
        : IPaginatedQueryHandler<Query, IReadOnlyList<Response>>
    {
        public async Task<Result<PaginatedResponse<IReadOnlyList<Response>>>> Handle(
            Query query,
            CancellationToken cancellationToken = default
        )
        {
            IQueryable<Program> programsQuery = context.Programs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                string searchTerm = query.SearchTerm.ToUpperInvariant();
                programsQuery = programsQuery.Where(p =>
                    EF.Functions.ILike(p.Name, $"%{searchTerm}%")
                    || EF.Functions.ILike(p.Description, $"%{searchTerm}%")
                );
            }

            if (query.Status.HasValue)
            {
                programsQuery = programsQuery.Where(p => p.ProgramStatus == query.Status.Value);
            }

            if (query.StartsAfter.HasValue)
            {
                programsQuery = programsQuery.Where(p => p.StartsAtUtc >= query.StartsAfter.Value);
            }

            if (query.StartsBefore.HasValue)
            {
                programsQuery = programsQuery.Where(p => p.StartsAtUtc <= query.StartsBefore.Value);
            }

            IQueryable<Response> projectedQuery = programsQuery
                .OrderByDescending(p => p.StartsAtUtc)
                .Select(p => new Response(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.StartsAtUtc,
                    p.EndsAtUtc,
                    p.ProgramStatus,
                    p.OwnerId,
                    context
                        .Users.Where(u => u.Id == p.OwnerId)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault()!,
                    p.Products.Select(pp => pp.ProductName).ToList()
                ));

            return await projectedQuery.ToPaginatedListAsync(query, cancellationToken);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapGet(
                    "/programs/search",
                    async (
                        int page,
                        int pageSize,
                        string? searchTerm,
                        ProgramStatus? status,
                        DateTime? startsAfter,
                        DateTime? startsBefore,
                        IPaginatedQueryHandler<Query, IReadOnlyList<Response>> handler,
                        CancellationToken cancellationToken
                    ) =>
                    {
                        var query = new Query(
                            page,
                            pageSize,
                            searchTerm,
                            status,
                            startsAfter,
                            startsBefore
                        );

                        Result<PaginatedResponse<IReadOnlyList<Response>>> result =
                            await handler.Handle(query, cancellationToken);

                        return result.Match(Results.Ok, ApiResults.Problem);
                    }
                )
                .WithTags("Programs");
    }
}
