using Vast.Common.Application.Exceptions;
using Vast.Common.Application.IntegrationEvents;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;
using Vast.Programs.Application.Features.ProgramStatistics;
using Vast.Transactions.IntegrationEvents;

namespace Vast.Programs.Application.IntegrationEventHandlers;

internal sealed class TransactionCreatedIntegrationEventHandler(
    IRequestHandler<UpsertProgramStatistic.Command, UpsertProgramStatistic.Response> handler
) : IntegrationEventHandler<TransactionCreatedIntegrationEvent>
{
    public override async Task Handle(
        TransactionCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        Result<UpsertProgramStatistic.Response> result = await handler.Handle(
            new UpsertProgramStatistic.Command(
                ProgramId: integrationEvent.ProgramId,
                TotalTransactions: 1,
                TotalCustomers: 1
            ),
            cancellationToken
        );

        if (result.IsFailure)
        {
            throw new VastException(nameof(UpsertProgramStatistic), result.Error);
        }
    }
}
