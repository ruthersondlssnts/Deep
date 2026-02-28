using System.ComponentModel.DataAnnotations;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.Users;

namespace Deep.Programs.Application.Features.Users;

public static class CreateUser
{
    public sealed record Command(
        [property: Required] Guid Id,
        [property: Required, MaxLength(100)] string FirstName,
        [property: Required, MaxLength(100)] string LastName,
        [property: Required] string Email,
        [property: Required, MinLength(1)] IReadOnlyCollection<string> Roles
    );

    public sealed record Response(Guid Id);

    public sealed class Handler(ProgramsDbContext context) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command c, CancellationToken ct = default)
        {
            User account = User.Create(c.Id, c.FirstName, c.LastName, c.Email, c.Roles);

            foreach (Role role in account.Roles)
            {
                context.Attach(role);
            }

            context.Users.Add(account);
            await context.SaveChangesAsync(ct);

            return new Response(account.Id);
        }
    }
}
