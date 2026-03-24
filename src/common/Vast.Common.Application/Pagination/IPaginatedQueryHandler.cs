using Vast.Common.Application.SimpleMediatR;

namespace Vast.Common.Application.Pagination;

public interface IPaginatedQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, PaginatedResponse<TResponse>>
    where TQuery : IPaginatedQuery<TResponse>;
