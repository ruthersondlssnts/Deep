using Deep.Accounts.IntegrationEvents;
using Deep.Common.Application.Exceptions;
using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Features.Users;

namespace Deep.Programs.Application.IntegrationEventHandlers;

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
            throw new DeepException(nameof(CreateUser), result.Error);
        }
    }
}
