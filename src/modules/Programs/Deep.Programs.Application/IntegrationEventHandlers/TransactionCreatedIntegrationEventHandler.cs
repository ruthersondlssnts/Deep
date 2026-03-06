using Deep.Common.Application.Exceptions;
using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Features.ProgramStatistics;
using Deep.Transactions.IntegrationEvents;

namespace Deep.Programs.Application.IntegrationEventHandlers;

internal sealed class TransactionCreatedIntegrationEventHandler(
    IRequestHandler<UpsertProgramStatistic.Command, UpsertProgramStatistic.Response> handler
) : IntegrationEventHandler<TransactionCreatedIntegrationEvent>
{
    public override async Task Handle(
        TransactionCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Result<UpsertProgramStatistic.Response> result = await handler.Handle(
            new UpsertProgramStatistic.Command(
                ProgramId: integrationEvent.ProgramId,
                TotalTransactions: 1,
                TotalCustomers: 1
            ),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new DeepException(nameof(UpsertProgramStatistic), result.Error);
        }
    }
}
