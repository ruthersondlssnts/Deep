using Deep.Programs.Domain.ProgramAssignments;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.Data.Repositories;

public class ProgramAssignmentRepository(ProgramsDbContext db) : IProgramAssignmentRepository
{
    public async Task<ProgramAssignment?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) => await db.ProgramAssignments.SingleOrDefaultAsync(acct => acct.Id == id, cancellationToken);

    public void InsertRange(IEnumerable<ProgramAssignment> assignments) =>
        db.ProgramAssignments.AddRange(assignments);

    public Task<List<ProgramAssignment>> GetAssignmentsByProgramId(
        Guid programId,
        CancellationToken ct
    ) => db.ProgramAssignments.Where(a => a.ProgramId == programId).ToListAsync(ct);
}
