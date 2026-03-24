using Microsoft.EntityFrameworkCore;

namespace Vast.Common.Application.Pagination;

public static class PaginationExtensions
{
    public static async Task<PaginatedResponse<IReadOnlyList<T>>> ToPaginatedListAsync<T>(
        this IQueryable<T> query,
        IPaginatedQuery<IReadOnlyList<T>> paginatedQuery,
        CancellationToken cancellationToken = default
    )
    {
        int totalCount = await query.CountAsync(cancellationToken);

        List<T> items = await query
            .Skip((paginatedQuery.Page - 1) * paginatedQuery.PageSize)
            .Take(paginatedQuery.PageSize)
            .ToListAsync(cancellationToken);

        return PaginatedResponse<IReadOnlyList<T>>.Create(items, paginatedQuery, totalCount);
    }
}
