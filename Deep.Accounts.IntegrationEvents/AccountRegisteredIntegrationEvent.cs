using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Domain;

namespace Deep.Accounts.IntegrationEvents;

public sealed class AccountRegisteredIntegrationEvent(
    Guid id,
    DateTime occurredAtUtc,
    Guid accountId,
    string email,
    string firstName,
    string lastName,
    Role role)
    : IntegrationEvent(id, occurredAtUtc)
{
    public Guid AccountId { get; } = accountId;
    public string Email { get; } = email;
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
    public Role Role { get; } = role;
}