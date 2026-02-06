using Deep.Common.Application.Exceptions;
using Deep.Common.Application.SimpleMediatR;
using Deep.Programs.Application.Features.ProgramStatistics;
using Deep.Transactions.IntegrationEvents;
using MassTransit;

namespace Deep.Programs.Application.IntegrationEventHandlers;

public sealed class TransactionCreatedIntegrationEventHandler(
    IRequestHandler<UpsertProgramStatistic.Command, UpsertProgramStatistic.Response> handler)
    : IConsumer<TransactionCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<TransactionCreatedIntegrationEvent> context)
    {
        var result = await handler.Handle(
            new UpsertProgramStatistic.Command(
                ProgramId: context.Message.ProgramId,
                TotalTransactions: context.Message.TotalTransactions,
                TotalCustomers: context.Message.TotalCustomers),
            context.CancellationToken);

        if (result.IsFailure)
            throw new DeepException(
                nameof(UpsertProgramStatistic),
                result.Error);
    }
}
