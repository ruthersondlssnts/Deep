// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Deep.Common.Domain;

public sealed record ValidationError(Error[] Errors)
: Error("General.Validation", "One or more validation errors occurred", ErrorType.Validation)
{
    public static ValidationError FromResults(IEnumerable<Result> results) =>
        new(results
            .Where(result => result.IsFailure)
            .Select(result => result.Error)
            .ToArray());
}
