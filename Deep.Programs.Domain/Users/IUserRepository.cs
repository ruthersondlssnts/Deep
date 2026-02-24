namespace Deep.Programs.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistWithRolesAsync(
        IReadOnlyCollection<(Guid UserId, string RoleName)> userRoles,
        CancellationToken cancellationToken = default
    );
    void Insert(User user);
}
