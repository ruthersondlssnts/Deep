using Vast.Common.Domain;

namespace Vast.Common.Application.SimpleMediatR;

public interface IRequestBus
{
    Task<Result<TResponse>> Send<TResponse>(object request, CancellationToken ct = default);
}
