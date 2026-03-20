using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.Programs;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.Features.Programs;

public static class ReserveStock
{
    public sealed record Command(
        Guid TransactionId,
        Guid ProgramId,
        string ProductSku,
        int Quantity
    );

    public sealed record Response(bool Reserved);

    public sealed class Handler(ProgramsDbContext context) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken ct = default)
        {
            Program? program = await context
                .Programs.Include(p => p.Products)
                .FirstOrDefaultAsync(p => p.Id == command.ProgramId, ct);

            if (program is null)
            {
                return ProgramErrors.NotFound(command.ProgramId);
            }

            if (!program.IsActive)
            {
                return ProgramErrors.ProgramNotActive;
            }

            Result reserveResult = program.ReserveStock(
                command.TransactionId,
                command.ProductSku,
                command.Quantity
            );

            await context.SaveChangesAsync(ct);

            if (reserveResult.IsFailure)
            {
                return reserveResult.Error;
            }

            return new Response(true);
        }
    }
}
