using Vast.Common.Domain;

namespace Vast.Accounts.Domain.Accounts;

public sealed class AccountRegisteredDomainEvent(Guid accountId) : DomainEvent
{
    public Guid AccountId { get; } = accountId;
}
