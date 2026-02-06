// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Common.Domain;

namespace Deep.Programs.Domain.Programs;

public class ProgramProduct
{
    public Guid ProgramId { get; internal set; }
    public string ProductName { get; internal set; } = string.Empty;


    private ProgramProduct() { }

    internal static Result<ProgramProduct> Create(Guid programId, string productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
            return ProgramErrors.InvalidProduct;

        return new ProgramProduct
        {
            ProgramId = programId,
            ProductName = productName
        };
    }
}
