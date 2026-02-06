// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Accounts.Domain.Accounts;
using Deep.Accounts.IntegrationEvents;
using Deep.Common.Application.DomainEvents;
using Deep.Common.Application.Exceptions;
using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Application.SimpleMediatR;

namespace Deep.Accounts.Application.Features.Accounts;

internal sealed class AccountRegisteredDomainEventHandler(
    IRequestHandler<GetAccount.Query, GetAccount.Response> getAccountHandler,
    IEventBus eventBus)
    : DomainEventHandler<AccountRegisteredDomainEvent>
{
    public override async Task Handle(
        AccountRegisteredDomainEvent notification,
        CancellationToken cancellationToken = default)
    {
        var result = await getAccountHandler.Handle(
            new GetAccount.Query(notification.AccountId),
            cancellationToken);

        if (result.IsFailure)
            throw new DeepException(
                nameof(GetAccount),
                result.Error);

        var account = result.Value;

        await eventBus.PublishAsync(
           new AccountRegisteredIntegrationEvent(
               notification.Id,
               notification.OccurredAtUtc,
               account.Id,
               account.Email,
               account.FirstName,
               account.LastName,
               account.Roles.Select(r => r.Name).ToList()),
           cancellationToken);
    }
}
