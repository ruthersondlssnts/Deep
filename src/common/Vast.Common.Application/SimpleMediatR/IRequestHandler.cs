using Vast.Common.Domain;

namespace Vast.Common.Application.SimpleMediatR;

public interface IRequestHandler<TRequest, TResponse>
{
    Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken = default);
}
