using Deep.Programs.Domain.ProgramAssignments;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.Data.Repositories;

public class ProgramAssignmentRepository(ProgramsDbContext db) : IProgramAssignmentRepository
{
    public async Task<ProgramAssignment?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) => await db.ProgramAssignments.SingleOrDefaultAsync(acct => acct.Id == id, cancellationToken);

    public async Task<List<ProgramAssignment>> GetActiveAssignmentsByProgramId(
        Guid programId,
        CancellationToken cancellationToken = default
    ) => await db.ProgramAssignments
        .Where(a => a.ProgramId == programId && a.IsActive)
        .ToListAsync(cancellationToken);

    public async Task<List<ProgramAssignment>> GetAssignmentsByProgramId(
        Guid programId,
        CancellationToken cancellationToken = default
    ) => await db.ProgramAssignments
        .Where(a => a.ProgramId == programId)
        .ToListAsync(cancellationToken);

    public void InsertRange(IEnumerable<ProgramAssignment> assignments) =>
        db.ProgramAssignments.AddRange(assignments);
}
