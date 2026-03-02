namespace Deep.Common.Application.Pagination;

public sealed class PaginatedResponse<TResponse>
{
    public required TResponse Data { get; init; }
    public required PageData PageData { get; init; }

    public static PaginatedResponse<TResponse> Create(
        TResponse data,
        IPaginatedQuery<TResponse> query,
        int totalCount) =>
        new()
        {
            Data = data,
            PageData = PageData.Create(query.Page, query.PageSize, totalCount)
        };

    public static PaginatedResponse<TResponse> Create(TResponse data, PageData pageData) =>
        new()
        {
            Data = data,
            PageData = pageData
        };
}
