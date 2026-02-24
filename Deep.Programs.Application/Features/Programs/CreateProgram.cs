using Deep.Common.Application.Api.ApiResults;
using Deep.Common.Application.Api.Endpoints;
using Deep.Common.Application.Authentication;
using Deep.Common.Application.SimpleMediatR;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.Programs;
using Deep.Programs.Domain.Users;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Deep.Programs.Application.Features.Programs;

public static class CreateProgram
{
    public sealed record ProgramUser(Guid UserId, string RoleName);

    public sealed record Command(
        string Name,
        string Description,
        DateTime StartsAtUtc,
        DateTime EndsAtUtc,
        IReadOnlyCollection<string> ProductNames,
        IReadOnlyCollection<ProgramUser> Users
    );

    public sealed record Response(Guid Id);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x.Description).NotEmpty();

            RuleFor(x => x.StartsAtUtc)
                .Must(x => x > DateTime.UtcNow)
                .WithMessage("Start date must be in the future.");

            RuleFor(x => x.EndsAtUtc)
                .GreaterThan(x => x.StartsAtUtc)
                .WithMessage("End date must be after start date.");

            RuleFor(x => x.ProductNames)
                .NotEmpty()
                .WithMessage("At least one product is required.");

            RuleFor(x => x.Users).NotEmpty().WithMessage("At least one user is required.");

            RuleForEach(x => x.Users)
                .ChildRules(user =>
                {
                    user.RuleFor(u => u.UserId).NotEmpty();

                    user.RuleFor(u => u.RoleName).NotEmpty();
                });
        }
    }

    public sealed class Handler(
        ProgramsDbContext context,
        IProgramRepository programRepository,
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor
    ) : IRequestHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command c, CancellationToken ct = default)
        {
            Guid currentUserId = httpContextAccessor.HttpContext!.User.GetUserId();

            var assignments = c.Users.Select(u => (u.UserId, u.RoleName)).Distinct().ToList();

            if (!await userRepository.ExistWithRolesAsync(assignments, ct))
            {
                return ProgramErrors.ProgramUserNotFound;
            }

            Result<Program> programResult = Program.Create(
                c.Name,
                c.Description,
                c.StartsAtUtc,
                c.EndsAtUtc,
                c.ProductNames,
                currentUserId,
                assignments
            );

            if (programResult.IsFailure)
            {
                return programResult.Error;
            }

            programRepository.Insert(programResult.Value);

            await context.SaveChangesAsync(ct);

            return new Response(programResult.Value.Id);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) =>
            app.MapPost(
                    "/programs",
                    async (
                        Command command,
                        IRequestHandler<Command, Response> handler,
                        CancellationToken ct
                    ) =>
                    {
                        Result<Response> result = await handler.Handle(command, ct);

                        return result.Match(
                            () => Results.Created($"/programs/{result.Value.Id}", result.Value),
                            ApiResults.Problem
                        );
                    }
                )
                .RequireAuthorization()
                .WithTags("Programs");
    }
}
