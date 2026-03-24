namespace Vast.Common.Application.Pagination;

public interface IPaginatedQuery<TResponse>
{
    int Page { get; }
    int PageSize { get; }
    Type ResponseType => typeof(TResponse);
}
