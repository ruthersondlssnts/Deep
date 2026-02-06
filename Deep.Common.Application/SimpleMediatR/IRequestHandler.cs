// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Common.Domain;

namespace Deep.Common.Application.SimpleMediatR;

public interface IRequestHandler<TRequest, TResponse>
{
    Task<Result<TResponse>> Handle(
        TRequest request,
        CancellationToken cancellationToken = default);
}


