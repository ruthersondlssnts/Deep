using Deep.Common.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deep.Common.Application.SimpleMediatR;

public interface IRequestHandler<TRequest, TResponse>
{
    Task<Result<TResponse>> Handle(
        TRequest request,
        CancellationToken cancellationToken = default);
}


