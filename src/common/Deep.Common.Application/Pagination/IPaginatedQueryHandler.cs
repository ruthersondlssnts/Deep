using Deep.Common.Application.SimpleMediatR;

namespace Deep.Common.Application.Pagination;

public interface IPaginatedQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, PaginatedResponse<TResponse>>
    where TQuery : IPaginatedQuery<TResponse>;
