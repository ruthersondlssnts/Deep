using Deep.Common.Domain;

namespace Deep.Common.Application.Exceptions;

public sealed class DeepException(
 string requestName,
 Error? error = default,
 Exception? innerException = default)
 : Exception("Application exception", innerException)
{
    public string RequestName { get; } = requestName;

    public Error? Error { get; } = error;
}
