// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Common.Application.IntegrationEvents;

namespace Deep.Accounts.IntegrationEvents;

public sealed class AccountRegisteredIntegrationEvent(
    Guid id,
    DateTime occurredAtUtc,
    Guid accountId,
    string email,
    string firstName,
    string lastName,
    IReadOnlyCollection<string> roles)
    : IntegrationEvent(id, occurredAtUtc)
{
    public Guid AccountId { get; } = accountId;
    public string Email { get; } = email;
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
    public IReadOnlyCollection<string> Roles { get; } = roles;
}
