using Vast.Accounts.Domain.Accounts;
using Vast.Accounts.IntegrationEvents;
using Vast.Common.Application.DomainEvents;
using Vast.Common.Application.Exceptions;
using Vast.Common.Application.IntegrationEvents;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;

namespace Vast.Accounts.Application.Features.Accounts;

internal sealed class AccountRegisteredDomainEventHandler(
    IRequestHandler<GetAccount.Query, GetAccount.Response> getAccountHandler,
    IEventBus eventBus
) : DomainEventHandler<AccountRegisteredDomainEvent>
{
    public override async Task Handle(
        AccountRegisteredDomainEvent notification,
        CancellationToken cancellationToken = default
    )
    {
        Result<GetAccount.Response> result = await getAccountHandler.Handle(
            new GetAccount.Query(notification.AccountId),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new VastException(nameof(GetAccount), result.Error);
        }

        GetAccount.Response account = result.Value;

        await eventBus.PublishAsync(
            new AccountRegisteredIntegrationEvent(
                notification.Id,
                notification.OccurredAtUtc,
                account.Id,
                account.Email,
                account.FirstName,
                account.LastName,
                account.Roles.Select(r => r.Name).ToList()
            ),
            cancellationToken
        );
    }
}
