using Deep.Common.Domain;

namespace Deep.Common.Application.Exceptions;

public sealed class DeepException : Exception
{
    public string RequestName { get; } = string.Empty;
    public Error? Error { get; }

    public DeepException(string requestName, Error? error, Exception? innerException = null)
        : base("Application exception", innerException)
    {
        RequestName = requestName;
        Error = error;
    }

    public DeepException() { }

    public DeepException(string message)
        : base(message) { }

    public DeepException(string message, Exception innerException)
        : base(message, innerException) { }
}
