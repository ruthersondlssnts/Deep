using Deep.Programs.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.Data.Repositories;

public class UserRepository(ProgramsDbContext db) : IUserRepository
{
    public async Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

    public void Insert(User user)
    {
        foreach (Role role in user.Roles)
        {
            db.Attach(role);
        }

        db.Users.Add(user);
    }
}
