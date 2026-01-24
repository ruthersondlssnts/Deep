using Deep.Common.Domain;

namespace Deep.Common.SimpleMediatR
{
    public interface IRequestBus
    {
        Task<Result<TResponse>> Send<TResponse>(
            object request,
            CancellationToken ct = default);
    }

  
}
