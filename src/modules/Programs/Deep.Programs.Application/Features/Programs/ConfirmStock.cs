using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.Programs;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.Features.Programs;

public static class ConfirmStock
{
    public sealed record Command(Guid ProgramId, string ProductSku, int Quantity);

    public sealed record Response(bool Confirmed);

    public sealed class Handler(ProgramsDbContext context) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(
            Command command,
            CancellationToken cancellationToken = default
        )
        {
            Program? program = await context
                .Programs.Include(p => p.Products)
                .FirstOrDefaultAsync(p => p.Id == command.ProgramId, cancellationToken);

            if (program is null)
            {
                return new Response(false);
            }

            Result result = program.ConfirmStockReservation(command.ProductSku, command.Quantity);

            if (result.IsFailure)
            {
                return result.Error;
            }

            await context.SaveChangesAsync(cancellationToken);

            return new Response(true);
        }
    }
}
