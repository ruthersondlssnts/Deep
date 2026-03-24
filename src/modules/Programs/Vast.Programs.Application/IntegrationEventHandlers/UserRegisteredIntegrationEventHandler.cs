using Vast.Accounts.IntegrationEvents;
using Vast.Common.Application.Exceptions;
using Vast.Common.Application.IntegrationEvents;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;
using Vast.Programs.Application.Features.Users;

namespace Vast.Programs.Application.IntegrationEventHandlers;

internal sealed class AccountRegisteredIntegrationEventHandler(
    IRequestHandler<CreateUser.Command, CreateUser.Response> handler
) : IntegrationEventHandler<AccountRegisteredIntegrationEvent>
{
    public override async Task Handle(
        AccountRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<CreateUser.Response> result = await handler.Handle(
            new CreateUser.Command(
                integrationEvent.AccountId,
                integrationEvent.FirstName,
                integrationEvent.LastName,
                integrationEvent.Email,
                integrationEvent.Roles
            ),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new VastException(nameof(CreateUser), result.Error);
        }
    }
}
