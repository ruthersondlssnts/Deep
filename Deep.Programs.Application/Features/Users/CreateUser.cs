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
        Role Role);

    public sealed record Response(Guid Id);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();

            RuleFor(x => x.Role).IsInEnum();

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }

    public sealed class Handler(ProgramsDbContext context)
        : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(
            Command c,
            CancellationToken ct)
        {
            var exists = await context.Users
                .AnyAsync(u => u.Id == c.Id || u.Email == c.Email, ct);

            if (exists)
                return UserErrors.UserAlreadyExists;

            var user = User.Create(
                c.Id,
                c.FirstName,
                c.LastName,
                c.Email,
                c.Role);

            context.Users.Add(user);
            await context.SaveChangesAsync(ct);

            return new Response(user.Id);
        }
    }

    //public sealed class Endpoint : IEndpoint
    //{
    //    public void MapEndpoint(IEndpointRouteBuilder app) =>
    //        app.MapPost("/users", async (
    //            Command command,
    //            IRequestHandler<Command, Response> handler,
    //            CancellationToken ct) =>
    //        {
    //            var result = await handler.Handle(command, ct);

    //            return result.Match(
    //                () => Results.Created(
    //                    $"/users/{result.Value.Id}",
    //                    result.Value),
    //                ApiResults.Problem);
    //        })
    //        .WithTags("Users");
    //}
}
