using Vast.Common.Domain;
using Microsoft.Extensions.Logging;

namespace Vast.Common.Application.SimpleMediatR;

public sealed partial class RequestLoggingPipelineBehavior<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> innerHandler,
    ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger
) : IRequestHandler<TRequest, TResponse>
    where TRequest : class
{
    private readonly IRequestHandler<TRequest, TResponse> _innerHandler = innerHandler;
    private readonly ILogger _logger = logger;

    public async Task<Result<TResponse>> Handle(
        TRequest request,
        CancellationToken cancellationToken = default
    )
    {
        Type requestType = typeof(TRequest);

        string requestName = requestType.DeclaringType is not null
            ? $"{requestType.DeclaringType.Name}.{requestType.Name}"
            : requestType.Name;

        string moduleName = GetModuleName(requestType.FullName!);

        using (_logger.BeginScope(new Dictionary<string, object> { ["Module"] = moduleName }))
        {
            LogProcessing(_logger, requestName);

            Result<TResponse> result = await _innerHandler.Handle(request, cancellationToken);

            if (result.IsSuccess)
            {
                LogCompleted(_logger, requestName);
            }
            else
            {
                LogFailed(_logger, requestName, result.Error);
            }

            return result;
        }
    }

    private static string GetModuleName(string requestFullName) => requestFullName.Split('.')[2];

    [LoggerMessage(
        EventId = 8000,
        Level = LogLevel.Information,
        Message = "Processing request {RequestName}"
    )]
    private static partial void LogProcessing(ILogger logger, string requestName);

    [LoggerMessage(
        EventId = 8001,
        Level = LogLevel.Information,
        Message = "Completed request {RequestName}"
    )]
    private static partial void LogCompleted(ILogger logger, string requestName);

    [LoggerMessage(
        EventId = 8002,
        Level = LogLevel.Error,
        Message = "Completed request {RequestName} with error {@Error}"
    )]
    private static partial void LogFailed(ILogger logger, string requestName, object? error);
}
