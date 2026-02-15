using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.Users;
using FluentValidation;

namespace Deep.Programs.Application.Features.Users;

public static class CreateUser
{
    public sealed record Command(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        IReadOnlyCollection<string> Roles
    );

    public sealed record Response(Guid Id);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty();

            RuleFor(x => x.Roles).NotNull().NotEmpty();

            RuleForEach(x => x.Roles).NotEmpty();

            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);

            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

            RuleFor(x => x.Email).NotEmpty();
        }
    }

    public sealed class Handler(ProgramsDbContext context, IUserRepository userRepository)
        : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command c, CancellationToken ct)
        {
            User account = User.Create(c.Id, c.FirstName, c.LastName, c.Email, c.Roles);

            foreach (Role role in account.Roles)
            {
                context.Attach(role);
            }

            userRepository.Insert(account);
            await context.SaveChangesAsync(ct);

            return new Response(account.Id);
        }
    }
}
