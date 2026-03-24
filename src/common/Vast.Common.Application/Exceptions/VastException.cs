using Vast.Common.Domain;

namespace Vast.Common.Application.Exceptions;

public sealed class VastException : Exception
{
    public string RequestName { get; } = string.Empty;
    public Error? Error { get; }

    public VastException(string requestName, Error? error, Exception? innerException = null)
        : base("Application exception", innerException)
    {
        RequestName = requestName;
        Error = error;
    }

    public VastException() { }

    public VastException(string message)
        : base(message) { }

    public VastException(string message, Exception innerException)
        : base(message, innerException) { }
}
