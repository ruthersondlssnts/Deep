using Deep.Programs.Domain.Programs;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.Data.Repositories;

public class ProgramRepository(ProgramsDbContext db) : IProgramRepository
{
    public async Task<Program?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db
            .Programs.Include(p => p.Products)
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<ProgramStatus?> GetStatusAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.Programs
            .Where(p => p.Id == id)
            .Select(p => (ProgramStatus?)p.ProgramStatus)
            .FirstOrDefaultAsync(cancellationToken);

    public void Insert(Program program) => db.Programs.Add(program);
}
