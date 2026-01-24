using Deep.Common.Domain;
using Microsoft.Extensions.Logging;

namespace Deep.Common.Application.SimpleMediatR;

public sealed class RequestLoggingPipelineBehavior<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> innerHandler,
    ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IRequestHandler<TRequest, TResponse>
    where TRequest : class
{
    public async Task<Result<TResponse>> Handle(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var requestType = typeof(TRequest);

        var requestName = requestType.DeclaringType is not null
            ? $"{requestType.DeclaringType.Name}.{requestType.Name}"
            : requestType.Name;

        var moduleName = GetModuleName(requestType.FullName!);

        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["Module"] = moduleName
        }))
        {
            logger.LogInformation(
                "Processing request {RequestName}",
                requestName);

            var result = await innerHandler.Handle(request, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation(
                    "Completed request {RequestName}",
                    requestName);
            }
            else
            {
                logger.LogError(
                    "Completed request {RequestName} with error {@Error}",
                    requestName,
                    result.Error);
            }

            return result;
        }
    }

    private static string GetModuleName(string requestFullName) =>
        requestFullName.Split('.')[2];
}
