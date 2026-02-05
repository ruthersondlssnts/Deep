using Deep.Common.Domain;

namespace Deep.Accounts.Domain.Accounts;

public sealed class AccountRegisteredDomainEvent(Guid accountId) : DomainEvent
{
    public Guid AccountId { get; } = accountId;
}