namespace Deep.Common.Application.Pagination;

public sealed record PageData
{
    private PageData() { }

    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public static PageData Create(int page, int pageSize, int totalCount)
    {
        int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PageData
        {
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalCount = totalCount,
        };
    }
}
