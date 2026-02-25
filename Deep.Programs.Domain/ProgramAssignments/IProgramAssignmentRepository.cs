namespace Deep.Programs.Domain.ProgramAssignments;

public interface IProgramAssignmentRepository
{
    Task<ProgramAssignment?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ProgramAssignment>> GetActiveAssignmentsByProgramId(Guid programId, CancellationToken cancellationToken = default);
    Task<List<ProgramAssignment>> GetAssignmentsByProgramId(Guid programId, CancellationToken cancellationToken = default);
    void InsertRange(IEnumerable<ProgramAssignment> assignments);
}
