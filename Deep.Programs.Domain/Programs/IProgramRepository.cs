namespace Deep.Programs.Domain.Programs;

public interface IProgramRepository
{
    Task<int> CountMatchingUserRolePairs(
        List<(Guid UserId, string RoleName)> users,
        CancellationToken cancellationToken = default
    );
}
