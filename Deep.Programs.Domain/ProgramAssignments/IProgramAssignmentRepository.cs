namespace Deep.Programs.Domain.ProgramAssignments;

public interface IProgramAssignmentRepository
{
    Task<ProgramAssignment?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    void InsertRange(IEnumerable<ProgramAssignment> assignments);

    Task<List<ProgramAssignment>> GetAssignmentsByProgramId(Guid programId, CancellationToken ct);
}
