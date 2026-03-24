using System.ComponentModel.DataAnnotations;
using Vast.Common.Application.SimpleMediatR;
using Vast.Common.Domain;
using Vast.Programs.Application.Data;
using Vast.Programs.Domain.Users;

namespace Vast.Programs.Application.Features.Users;

public static class CreateUser
{
    public sealed record Command(
        [Required] Guid Id,
        [Required, MaxLength(100)] string FirstName,
        [Required, MaxLength(100)] string LastName,
        [Required] string Email,
        [Required, MinLength(1)] IReadOnlyCollection<string> Roles
    );

    public sealed record Response(Guid Id);

    public sealed class Handler(ProgramsDbContext context) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(
            Command c,
            CancellationToken cancellationToken = default
        )
        {
            User account = User.Create(c.Id, c.FirstName, c.LastName, c.Email, c.Roles);

            foreach (Role role in account.Roles)
            {
                context.Attach(role);
            }

            context.Users.Add(account);
            await context.SaveChangesAsync(cancellationToken);

            return new Response(account.Id);
        }
    }
}
