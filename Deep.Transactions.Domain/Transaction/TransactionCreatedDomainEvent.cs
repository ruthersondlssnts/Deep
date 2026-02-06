// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Common.Domain;

namespace Deep.Transactions.Domain.Transaction
{
    public sealed class TransactionCreatedDomainEvent(
        Guid transactionId, Guid programId) : DomainEvent
    {
        public Guid TransactionId { get; } = transactionId;
        public Guid ProgramId { get; } = programId;
    }
}
