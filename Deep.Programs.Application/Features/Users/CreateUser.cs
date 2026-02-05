using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.Users;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Deep.Programs.Application.Features.Users;

public static class CreateUser
{
    public sealed record Command(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        IReadOnlyCollection<string> Roles);

    public sealed record Response(Guid Id);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
               .NotNull()
               .NotEmpty();

            RuleFor(x => x.Roles)
                .NotNull()
                .NotEmpty();
            RuleForEach(x => x.Roles)
                .NotEmpty();

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty();
        }
    }

    public sealed class Handler(ProgramsDbContext context)
        : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(
            Command c,
            CancellationToken ct)
        {
            var roles = new List<Role>();
            foreach (var roleName in c.Roles)
            {
                if (!Role.TryFromName(roleName, out var role))
                    return UserErrors.InvalidRole;
                roles.Add(role);
            }

            var account = User.Create(
                c.Id,
                c.FirstName,
                c.LastName,
                c.Email,
                roles);

            context.Users.Add(account);
            await context.SaveChangesAsync(ct);

            return new Response(account.Id);
        }
    }
}
