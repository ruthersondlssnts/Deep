using Vast.Common.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Vast.Common.Application.SimpleMediatR;

public sealed class RequestBus(IServiceProvider provider) : IRequestBus
{
    public async Task<Result<TResponse>> Send<TResponse>(
        object request,
        CancellationToken ct = default
    )
    {
        Type requestType = request.GetType();

        Type handlerType = typeof(IRequestHandler<,>).MakeGenericType(
            requestType,
            typeof(TResponse)
        );

        dynamic handler = provider.GetRequiredService(handlerType);

        return await handler.Handle((dynamic)request, ct);
    }
}
