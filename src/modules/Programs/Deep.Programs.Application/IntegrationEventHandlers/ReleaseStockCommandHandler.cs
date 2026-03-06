using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.Programs;
using Deep.Programs.IntegrationEvents;
using Deep.Transactions.IntegrationEvents;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.IntegrationEventHandlers;

internal sealed class ReleaseStockCommandHandler(
    ProgramsDbContext context,
    IEventBus eventBus
) : IntegrationEventHandler<ReleaseStockCommand>
{
    public override async Task Handle(
        ReleaseStockCommand command,
        CancellationToken cancellationToken = default)
    {
        Program? program = await context.Programs
            .Include(p => p.Products)
            .FirstOrDefaultAsync(p => p.Id == command.ProgramId, cancellationToken);

        if (program is null)
        {
            // Log error - program doesn't exist
            return;
        }

        Result result = program.ReleaseReservedStock(command.ProductSku, command.Quantity);

        if (result.IsSuccess)
        {
            await context.SaveChangesAsync(cancellationToken);

            await eventBus.PublishAsync(
                new StockReleasedIntegrationEvent(
                    Guid.CreateVersion7(),
                    DateTime.UtcNow,
                    command.TransactionId,
                    command.ProgramId,
                    command.ProductSku,
                    command.Quantity),
                cancellationToken);
        }
    }
}
