// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Common.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Common.Application.SimpleMediatR;

public sealed class RequestBus(IServiceProvider provider)
: IRequestBus
{
    public async Task<Result<TResponse>> Send<TResponse>(
        object request,
        CancellationToken ct = default)
    {
        var requestType = request.GetType();

        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, typeof(TResponse));

        dynamic handler = provider.GetRequiredService(handlerType);

        return await handler.Handle((dynamic)request, ct);
    }
}


