namespace Deep.Programs.Domain.Programs;

public interface IProgramRepository
{
    Task<Program?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    void Insert(Program program);
}
