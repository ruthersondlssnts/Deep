namespace Deep.Common.Application.Pagination;

public interface IPaginatedQuery<TResponse>
{
    int Page { get; }
    int PageSize { get; }
}
