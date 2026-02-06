// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Common.Application.IntegrationEvents;

namespace Deep.Transactions.IntegrationEvents;

public sealed class TransactionCreatedIntegrationEvent(
    Guid id,
    DateTime occurredAtUtc,
    int totalTransactions,
    int totalCustomers,
    Guid programId)
: IntegrationEvent(id, occurredAtUtc)
{
    public int TotalTransactions { get; } = totalTransactions;
    public int TotalCustomers { get; } = totalCustomers;
    public Guid ProgramId { get; } = programId;
}
