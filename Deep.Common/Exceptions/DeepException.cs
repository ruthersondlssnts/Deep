using Deep.Common.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deep.Common.Exceptions
{
    public sealed class DeepException(
     string requestName,
     Error? error = default,
     Exception? innerException = default)
     : Exception("Application exception", innerException)
    {
        public string RequestName { get; } = requestName;

        public Error? Error { get; } = error;
    }
}
