using Deep.Common.Domain;

namespace Deep.Common.Application.SimpleMediatR;

public interface IRequestBus
{
    Task<Result<TResponse>> Send<TResponse>(object request, CancellationToken ct = default);
}
