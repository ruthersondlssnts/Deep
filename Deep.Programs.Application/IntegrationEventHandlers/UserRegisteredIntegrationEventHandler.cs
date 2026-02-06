using Deep.Accounts.IntegrationEvents;
using Deep.Common.Application.Exceptions;
using Deep.Common.Application.SimpleMediatR;
using Deep.Programs.Application.Features.Users;
using MassTransit;

namespace Deep.Programs.Application.IntegrationEventHandlers;

public sealed class UserRegisteredIntegrationEventConsumer(
    IRequestHandler<CreateUser.Command, CreateUser.Response> handler
) : IConsumer<AccountRegisteredIntegrationEvent>
{
    public async Task Consume(ConsumeContext<AccountRegisteredIntegrationEvent> context)
    {
        var result = await handler.Handle(
            new CreateUser.Command(
                context.Message.AccountId,
                context.Message.FirstName,
                context.Message.LastName,
                context.Message.Email,
                context.Message.Roles
            ),
            context.CancellationToken
        );

        if (result.IsFailure)
            throw new DeepException(nameof(CreateUser), result.Error);
    }
}
