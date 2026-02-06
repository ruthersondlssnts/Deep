// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Common.Domain;

namespace Deep.Accounts.Domain.Accounts;

public sealed class AccountRegisteredDomainEvent(Guid accountId) : DomainEvent
{
    public Guid AccountId { get; } = accountId;
}
