using Deep.Common.Application.IntegrationEvents;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.Programs;
using Deep.Transactions.IntegrationEvents;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.IntegrationEventHandlers;

internal sealed class ConfirmStockCommandHandler(
    ProgramsDbContext context
) : IntegrationEventHandler<ConfirmStockCommand>
{
    public override async Task Handle(
        ConfirmStockCommand command,
        CancellationToken cancellationToken = default)
    {
        Program? program = await context.Programs
            .Include(p => p.Products)
            .FirstOrDefaultAsync(p => p.Id == command.ProgramId, cancellationToken);

        if (program is null)
        {
            return;
        }

        Result result = program.ConfirmStockReservation(command.ProductSku, command.Quantity);

        if (result.IsSuccess)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
