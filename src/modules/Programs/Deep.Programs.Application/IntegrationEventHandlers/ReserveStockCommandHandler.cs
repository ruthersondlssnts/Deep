using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.Programs;
using Deep.Programs.IntegrationEvents;
using Deep.Transactions.IntegrationEvents;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.IntegrationEventHandlers;

internal sealed class ReserveStockCommandHandler(
    ProgramsDbContext context,
    IEventBus eventBus
) : IntegrationEventHandler<ReserveStockCommand>
{
    public override async Task Handle(
        ReserveStockCommand command,
        CancellationToken cancellationToken = default)
    {
        Program? program = await context.Programs
            .Include(p => p.Products)
            .FirstOrDefaultAsync(p => p.Id == command.ProgramId, cancellationToken);

        if (program is null)
        {
            await PublishFailure(command, $"Program {command.ProgramId} not found", cancellationToken);
            return;
        }

        if (!program.IsActive)
        {
            await PublishFailure(command, "Program is not active", cancellationToken);
            return;
        }

        Result<ProgramProduct> productResult = program.GetProduct(command.ProductSku);
        if (productResult.IsFailure)
        {
            await PublishFailure(command, productResult.Error.Description, cancellationToken);
            return;
        }

        ProgramProduct product = productResult.Value;
        Result reserveResult = program.ReserveStock(command.ProductSku, command.Quantity);

        if (reserveResult.IsFailure)
        {
            await PublishFailure(command, reserveResult.Error.Description, cancellationToken);
            return;
        }

        await context.SaveChangesAsync(cancellationToken);

        await eventBus.PublishAsync(
            new StockReservedIntegrationEvent(
                Guid.CreateVersion7(),
                DateTime.UtcNow,
                command.TransactionId,
                command.ProgramId,
                command.ProductSku,
                command.Quantity,
                product.UnitPrice),
            cancellationToken);
    }

    private async Task PublishFailure(
        ReserveStockCommand command,
        string reason,
        CancellationToken cancellationToken)
    {
        await eventBus.PublishAsync(
            new StockReservationFailedIntegrationEvent(
                Guid.CreateVersion7(),
                DateTime.UtcNow,
                command.TransactionId,
                command.ProgramId,
                command.ProductSku,
                command.Quantity,
                reason),
            cancellationToken);
    }
}
