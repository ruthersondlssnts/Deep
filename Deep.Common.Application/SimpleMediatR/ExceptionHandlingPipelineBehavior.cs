using Deep.Common.Application.Exceptions;
using Deep.Common.Domain;
using Microsoft.Extensions.Logging;

namespace Deep.Common.Application.SimpleMediatR;

public sealed class ExceptionHandlingPipelineBehavior<TRequest, TResponse>
    : IRequestHandler<TRequest, TResponse>
    where TRequest : class
{
    private readonly IRequestHandler<TRequest, TResponse> _inner;
    private readonly ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingPipelineBehavior(
        IRequestHandler<TRequest, TResponse> inner,
        ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _inner.Handle(request, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled exception for {RequestName}",
                typeof(TRequest).Name);

            throw new DeepException(
                typeof(TRequest).Name,
                innerException: exception);
        }
    }
}

